using FFmpeg.NET;
using FFmpeg.NET.Enums;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using IDBrowserServiceCore.Data.PostgresHelpers;
using IDBrowserServiceCore.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Code
{
    public class ConsoleFunctions
    {
        public static bool? isWindows;
        public static bool IsWindows
        {
            get
            {
                if (isWindows == null)
                    isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                return isWindows.Value;
            }
        }

        /// <summary>
        /// Transcode's all videos of a site
        /// </summary>
        /// <param name="configuration">Json configuration of IDBrowserService as IDBrowserConfiguration class</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <param name="siteName">Site name to transcode</param>
        /// <param name="videoSize">Video size to transcode. (e.g. Hd480, Hd720, Hd1080)</param>
        /// <param name="taskCount">FFmpeg task count (Default 2)</param>
        /// <param name="logLevel">Serilog log level</param>
        public static void TranscodeAllVideos(IDBrowserConfiguration configuration, CancellationToken cancellationToken,
            string siteName, string videoSize, Serilog.Events.LogEventLevel logLevel, int taskCount)
        {
            LoggingLevelSwitch loggingLevelSwitch = new LoggingLevelSwitch
            {
                MinimumLevel = logLevel
            };

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch)
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", "ConsoleFunctions.log"))
                .CreateLogger();

            SiteSettings siteSettings = configuration.Sites[siteName];
            
            IDImagerDB db = new IDImagerDB(GetIDImagerDBOptionsBuilder<IDImagerDB>(siteSettings.ConnectionStrings.DBType,
                siteSettings.ConnectionStrings.IDImager).Options);

            var query = db.idCatalogItem
                .Include(x => x.idFilePath)
                .Where(x => configuration.VideoFileExtensions.Contains(x.idFileType));

            ProgressTaskFactory progressTaskFactory = new ProgressTaskFactory(taskCount, 100, 50, Log.Logger);

            Task windowChangeListenerTask = new Task(() =>
            {
                int width = Console.WindowWidth;
                int height = Console.WindowHeight;

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (width != Console.WindowWidth || height != Console.WindowHeight)
                    {
                        width = Console.WindowWidth;
                        height = Console.WindowHeight;

                        progressTaskFactory.RedrawConsoleWindows(100, 50);
                    }

                    cancellationToken.WaitHandle.WaitOne(100);
                }
            }, cancellationToken);

            windowChangeListenerTask.Start();

            if (!IsWindows)
            {
                // Log output scrolling not supported at the moment because Console.MoveBufferArea is currently not supported under Linux and MacOs.
                // But we can write a single infoline.
                progressTaskFactory.WriteLog("Log output in console not supported under Linux at the moment. Please take a look at the ConsoleFunctions.log in Logs directory.",
                    true, LogEventLevel.Information);
            }

            List<TranscodeVideoBatchInfo> listTranscodeVideoBatch = new List<TranscodeVideoBatchInfo>();

            foreach (idCatalogItem catalogItem in query)
            {
                catalogItem.GetHeightAndWidth(out int originalVideoWidth, out int originalVideoHeight);

                string strTranscodeFilePath = StaticFunctions.GetTranscodeFilePath(catalogItem.GUID,
                    siteSettings.ServiceSettings.TranscodeDirectory, videoSize);
                FileInfo transcodeFileInfo = new FileInfo(strTranscodeFilePath);

                if (!transcodeFileInfo.Exists)
                {
                    string strOriginalFilePath = StaticFunctions.GetImageFilePath(catalogItem, siteSettings.ServiceSettings.FilePathReplace);

                    TranscodeVideoBatchInfo transcodeVideoBatchInfo = new TranscodeVideoBatchInfo(catalogItem.GUID, originalVideoWidth, originalVideoHeight,
                        strOriginalFilePath, strTranscodeFilePath);

                    listTranscodeVideoBatch.Add(transcodeVideoBatchInfo);
                }
            }

            int intTotalCount = listTranscodeVideoBatch.Count();
            int intCounter = 1;
            TranscodeVideoBatchInfo lastTranscodeVideoBatch = listTranscodeVideoBatch.LastOrDefault();

            foreach (TranscodeVideoBatchInfo transcodeVideoBatchInfo in listTranscodeVideoBatch)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                string strTranscodeFilePath = StaticFunctions.GetTranscodeFilePath(transcodeVideoBatchInfo.GUID,
                    siteSettings.ServiceSettings.TranscodeDirectory, videoSize);

                StaticFunctions.GetTranscodeVideoSize(videoSize, transcodeVideoBatchInfo.VideoWidth, transcodeVideoBatchInfo.VideoHeight,
                    out VideoSize targetVideoSize, out int targetVideoWidth, out int targetVideoHeight);

                var conversionOptions = StaticFunctions.GetConversionOptions(targetVideoSize, transcodeVideoBatchInfo.VideoWidth,
                    transcodeVideoBatchInfo.VideoHeight);

                progressTaskFactory.WriteLog(string.Format("Transcoding file {0} of {1} with guid {2} and path \"{3}\" from resolution {4}x{5} to {6}x{7} ",
                        intCounter, intTotalCount, transcodeVideoBatchInfo.GUID, transcodeVideoBatchInfo.VideoFileInfo.FullName, transcodeVideoBatchInfo.VideoWidth,
                        transcodeVideoBatchInfo.VideoHeight, targetVideoWidth, targetVideoHeight),
                    IsWindows, LogEventLevel.Information);

                ProgressTask progressTask = progressTaskFactory.GetIdleProgressTask();

                if (cancellationToken.IsCancellationRequested)
                    return;
                    
                if (progressTask == null)
                {
                    progressTask = progressTaskFactory
                        .WaitForAnyTask()
                        .Result;
                }

                if (cancellationToken.IsCancellationRequested)
                    return;
               
                string progressBarText = string.Format("{0} ({1} of {2})", transcodeVideoBatchInfo.TranscodeFileInfo.Name, intCounter, intTotalCount);

                progressTask.Task = new Task(() => {
                    TranscodeVideoProgressTask(transcodeVideoBatchInfo, videoSize, cancellationToken, progressTask, progressBarText);
                });

                progressTask.Task.Start();

                // If we are on the last item we have to wait for all tasks to complete
                if (transcodeVideoBatchInfo == lastTranscodeVideoBatch)
                {
                    progressTaskFactory.WaitForAllTasks();
                }
                
                intCounter++;
            }

            Log.CloseAndFlush();
        }

        /// <summary>
        /// Transcodes single video with console progresbar updates.
        /// </summary>
        /// <param name="transcodeVideoBatchInfo">Transcode video batch info</param>
        /// <param name="videoSize">Video size</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <param name="progressTask">Progress task</param>
        /// <param name="progressBarText">Progress bar text</param>
        public static void TranscodeVideoProgressTask(TranscodeVideoBatchInfo transcodeVideoBatchInfo, string videoSize,
            CancellationToken cancellationToken, ProgressTask progressTask, string progressBarText)
        {
            try
            {
                Engine ffmpegEngine = StaticFunctions.GetFFmpegEngine();

                ffmpegEngine.Error += (sender, eventArgs) =>
                {
                    progressTask.ProgressTaskFactory.WriteLog(string.Format("FFmpeg error on file \"{0}\" => {1}",
                            transcodeVideoBatchInfo.VideoFileInfo.FullName, eventArgs.Exception.ToString()),
                        IsWindows, LogEventLevel.Error);
                };

                ffmpegEngine.Progress += (sender, eventArgs) =>
                {
                    int intProgress = (int)Math.Round(eventArgs.ProcessedDuration * 100 / eventArgs.TotalDuration);
                    intProgress = Math.Max(0, Math.Min(100, intProgress));
                    progressTask.RefreshProgressBar(intProgress, progressBarText);
                };

                progressTask.RefreshProgressBar(0, progressBarText);

                Task task = StaticFunctions.TranscodeVideo(ffmpegEngine, cancellationToken, transcodeVideoBatchInfo.VideoFileInfo.FullName,
                    transcodeVideoBatchInfo.TranscodeFileInfo.FullName, videoSize, transcodeVideoBatchInfo.VideoWidth, transcodeVideoBatchInfo.VideoHeight);

                task.Wait();

                transcodeVideoBatchInfo.TranscodeFileInfo.Refresh();

                if (!transcodeVideoBatchInfo.TranscodeFileInfo.Exists)
                {
                    progressTask.ProgressTaskFactory.WriteLog(string.Format("Transcoding on file \"{0}\" failed, file does not exist.",
                            transcodeVideoBatchInfo.VideoFileInfo.FullName),
                        IsWindows, LogEventLevel.Error);
                    progressTask.RefreshProgressBar(0, progressBarText + " (failed)");
                }
                else if (transcodeVideoBatchInfo.TranscodeFileInfo.Length == 0)
                {
                    transcodeVideoBatchInfo.TranscodeFileInfo.Delete();
                    progressTask.ProgressTaskFactory.WriteLog(string.Format("Transcoding failed on file \"{0}\", file size is zero. Unfinished transcoded file \"{1}\" deleted.",
                            transcodeVideoBatchInfo.VideoFileInfo.FullName, transcodeVideoBatchInfo.TranscodeFileInfo.FullName),
                        IsWindows, LogEventLevel.Error);
                    progressTask.RefreshProgressBar(0, progressBarText + " (failed)");
                }
                else
                {
                    progressTask.RefreshProgressBar(100, progressBarText);
                }
            }
            catch (Exception ex)
            {
                // Wait for file to be unlocked
                Thread.Sleep(2000);
                transcodeVideoBatchInfo.TranscodeFileInfo.Refresh();

                if (transcodeVideoBatchInfo.TranscodeFileInfo != null && transcodeVideoBatchInfo.TranscodeFileInfo.Exists)
                {
                    transcodeVideoBatchInfo.TranscodeFileInfo.Delete();
                    progressTask.ProgressTaskFactory.WriteLog(string.Format("Unfinished transcoded file \"{0}\" deleted.",
                            transcodeVideoBatchInfo.TranscodeFileInfo.FullName),
                        IsWindows, LogEventLevel.Error);
                }

                if (ex.GetType() == typeof(AggregateException)
                    && ex.InnerException != null
                    && ex.InnerException.GetType() == typeof(TaskCanceledException))
                {

                    progressTask.ProgressTaskFactory.WriteLog("Transcoding cancelled", IsWindows, Serilog.Events.LogEventLevel.Information);
                }
                else
                {
                    progressTask.ProgressTaskFactory.WriteLog(ex.ToString(), IsWindows, Serilog.Events.LogEventLevel.Information);
                }

                return;
            }
        }

        /// <summary>
        /// Generates thumbnails based on parameters
        /// </summary>
        /// <param name="configuration">Json configuration of IDBrowserService as IDBrowserConfiguration class</param>
        /// <param name="siteName">Site name to generate thumbnails</param>
        /// <param name="fromDateTime">From date filter</param>
        /// <param name="toDateTime">To date filter</param>
        /// <param name="fileFilter">File type filter</param>
        /// <param name="imageGuid">Generate single image guid</param>
        /// <param name="overwrite">Overwrite existing thumbnails</param>
        public async static void GenerateThumbnails(IDBrowserConfiguration configuration, string siteName, DateTime fromDateTime,
            DateTime toDateTime, string fileFilter, string imageGuid, bool overwrite)
        {
            SiteSettings siteSettings = configuration.Sites[siteName];

            var optionsBuilder = new DbContextOptionsBuilder<IDImagerDB>();

            if (siteSettings.ConnectionStrings.DBType.Equals("MsSql"))
            {
                optionsBuilder.UseSqlServer(siteSettings.ConnectionStrings.IDImager);
            }
            else if (siteSettings.ConnectionStrings.DBType.Equals("Postgres"))
            {
                optionsBuilder.UseNpgsql(siteSettings.ConnectionStrings.IDImager);
                optionsBuilder.ReplaceService<ISqlGenerationHelper, PostgresSqlGenerationHelper>();
            }

            IDImagerDB db = new IDImagerDB(GetIDImagerDBOptionsBuilder<IDImagerDB>(siteSettings.ConnectionStrings.DBType,
                siteSettings.ConnectionStrings.IDImager).Options);
            IDImagerThumbsDB dbThumbs = new IDImagerThumbsDB(GetIDImagerDBOptionsBuilder<IDImagerThumbsDB>(siteSettings.ConnectionStrings.DBType,
                siteSettings.ConnectionStrings.IDImagerThumbs).Options);

            var queryCatalogItem = from catalogItem in db.idCatalogItem.Include("idFilePath")
                                   where configuration.ImageFileExtensions.Contains(catalogItem.idFileType)
                                   || configuration.VideoFileExtensions.Contains(catalogItem.idFileType)
                                   select catalogItem;

            if (fromDateTime != DateTime.MinValue)
                queryCatalogItem = queryCatalogItem.Where(x => x.idCreated >= fromDateTime);

            if (toDateTime != DateTime.MinValue)
                queryCatalogItem = queryCatalogItem.Where(x => x.idCreated <= toDateTime);

            if (!string.IsNullOrEmpty(fileFilter))
                queryCatalogItem = queryCatalogItem.Where(x => x.idFileType.ToLower().Equals(fileFilter.ToLower()));

            if (!string.IsNullOrEmpty(imageGuid))
                queryCatalogItem = queryCatalogItem.Where(x => x.GUID.Equals(imageGuid, StringComparison.OrdinalIgnoreCase));

            int intCountCatalogItem = queryCatalogItem.Count();
            int intCatalogItemCounter = 0;
            int intThumbnailsGenerated = 0;

            foreach (idCatalogItem catalogItem in queryCatalogItem)
            {
                List<String> typesToGenerate = new List<String>();
                List<idThumbs> idThumbsT = dbThumbs.idThumbs.Where(x => x.ImageGUID == catalogItem.GUID && x.idType.Equals("T")).ToList();
                List<idThumbs> idThumbsM = dbThumbs.idThumbs.Where(x => x.ImageGUID == catalogItem.GUID && x.idType.Equals("M")).ToList();
                List<idThumbs> idThumbsR = dbThumbs.idThumbs.Where(x => x.ImageGUID == catalogItem.GUID && x.idType.Equals("R")).ToList();

                if (idThumbsT.Count() == 0)
                    typesToGenerate.Add("T");

                if (idThumbsM.Count() == 0)
                    typesToGenerate.Add("M");

                if (catalogItem.idHasRecipe > 0 && idThumbsR.Count() == 0)
                    typesToGenerate.Add("R");

                if (overwrite)
                {
                    foreach (idThumbs thumb in idThumbsT)
                        dbThumbs.idThumbs.Remove(thumb);

                    foreach (idThumbs thumb in idThumbsM)
                        dbThumbs.idThumbs.Remove(thumb);

                    foreach (idThumbs thumb in idThumbsR)
                        dbThumbs.idThumbs.Remove(thumb);

                    typesToGenerate.Clear();
                    typesToGenerate.Add("T");
                    typesToGenerate.Add("M");
                    if (catalogItem.idHasRecipe > 0)
                        typesToGenerate.Add("R");
                }

                if (typesToGenerate.Count() > 0)
                {
                    try
                    {
                        SaveImageThumbnailResult result = await StaticFunctions.SaveImageThumbnail(catalogItem, db, dbThumbs, typesToGenerate,
                            siteSettings.ServiceSettings);
                        foreach (Exception ex in result.Exceptions)
                        {
                            LogGenerateThumbnailsException(ex);
                            Console.WriteLine(ex.ToString());
                        }

                        if (result.Exceptions.Count > 0)
                            LogGenerateThumbnailsFailedCatalogItem(catalogItem);

                        intThumbnailsGenerated += result.ImageStreams.Count();
                    }
                    catch (Exception e)
                    {
                        LogGenerateThumbnailsException(e);
                        LogGenerateThumbnailsFailedCatalogItem(catalogItem);
                        Console.WriteLine(e.ToString());
                    }
                }

                intCatalogItemCounter += 1;

                Console.CursorLeft = 0;
                Console.Write(String.Format("{0} of {1} catalogitems checked. {2} thumbnails generated", intCatalogItemCounter, intCountCatalogItem, intThumbnailsGenerated));
            }
        }

        public static DbContextOptionsBuilder<TContext> GetIDImagerDBOptionsBuilder<TContext>(string dbType,
            string connectionString) where TContext : DbContext
        {
            DbContextOptionsBuilder<TContext> optionsBuilder = new DbContextOptionsBuilder<TContext>();
            StaticFunctions.SetDbContextOptions(optionsBuilder, dbType, connectionString);
            return optionsBuilder;
        }

        private static void LogGenerateThumbnailsException(Exception e)
        {
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ThumbnailGeneratorErrorLog.txt"), e.ToString() + "\r\n-----------------------------------\r\n");
        }

        private static void LogGenerateThumbnailsFailedCatalogItem(idCatalogItem catalogItem)
        {
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ThumbnailGeneratorFailedCatalogItems.txt"), catalogItem.GUID + "\r\n");
        }
    }
}
