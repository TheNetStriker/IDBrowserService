using ComponentAce.Compression.Libs.zlib;
using FFmpeg.NET;
using FFmpeg.NET.Enums;
using IDBrowserServiceCore.Code.XmpRecipe;
using IDBrowserServiceCore.Data;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using IDBrowserServiceCore.Data.PostgresHelpers;
using IDBrowserServiceCore.Settings;
using ImageMagick;
using Konsole;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IDBrowserServiceCore.Code
{
    public class StaticFunctions
    {
        public static FileStream GetImageFileStream(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return fileStream;
        }

        public static String GetImageFilePath(idCatalogItem catalogItem, List<FilePathReplaceSettings> filePathReplaceSettings)
        {
            string strFilePath = catalogItem.idCache_FilePath.FilePath;

            if (filePathReplaceSettings != null)
            {
                foreach (FilePathReplaceSettings settings in filePathReplaceSettings)
                {
                    string strPathMatch = settings.PathMatch;
                    string strPathReplace = settings.PathReplace;
                    if (!string.IsNullOrEmpty(strPathMatch) && !string.IsNullOrEmpty(strPathReplace))
                        strFilePath = strFilePath.Replace(strPathMatch, strPathReplace, StringComparison.CurrentCultureIgnoreCase);

                    if (Path.DirectorySeparatorChar != '\\')
                        strFilePath = strFilePath.Replace('\\', Path.DirectorySeparatorChar);
                }
            }

            return Path.Combine(strFilePath, catalogItem.FileName);
        }

        public async static Task<SaveImageThumbnailResult> SaveImageThumbnail(idCatalogItem catalogItem, IDImagerDB db, IDImagerThumbsDB dbThumbs,
            List<string> types, ServiceSettings serviceSettings)
        {
            SaveImageThumbnailResult result = new SaveImageThumbnailResult();
            Stream imageStream = null;
            String filePath = null;

            try
            {
                filePath = GetImageFilePath(catalogItem, serviceSettings.FilePathReplace);
                imageStream = GetImageFileStream(filePath);

                foreach (String type in types)
                {
                    uint imageWidth;
                    uint imageHeight;

                    if (type.Equals("T"))
                    {
                        imageWidth = 160;
                        imageHeight = 120;
                    }
                    else
                    {
                        imageWidth = serviceSettings.MThumbmailWidth;
                        imageHeight = serviceSettings.MThumbnailHeight;
                    }

                    XmpRecipeContainer xmpRecipeContainer = null;
                    if (type.Equals("T") || type.Equals("R"))
                    {
                        if (catalogItem.idHasRecipe > 0)
                        {
                            XDocument recipeXDocument = await GetRecipeXDocument(db, catalogItem);
                            xmpRecipeContainer = XmpRecipeHelper.ParseXmlRecepie(recipeXDocument);
                        }
                    }

                    MemoryStream resizedImageStream = new MemoryStream();
                    MagickReadSettings magickReadSettings = null;

                    if (Enum.TryParse<MagickFormat>(catalogItem.idFileType, true, out MagickFormat magickFormat))
                    {
                        magickReadSettings = new MagickReadSettings { Format = magickFormat, FrameCount = 1 };
                    }

                    imageStream.Position = 0;

                    MagickImage image = new MagickImage(imageStream, magickReadSettings)
                    {
                        Format = MagickFormat.Jpeg
                    }; 

                    image.Resize(imageWidth, imageHeight);

                    if (xmpRecipeContainer != null)
                        XmpRecipeHelper.ApplyXmpRecipe(xmpRecipeContainer, image);

                    image.Write(resizedImageStream);
                    resizedImageStream.Position = 0;

                    bool boolThumbExists = await dbThumbs.idThumbs
                        .AnyAsync(x => x.ImageGUID == catalogItem.GUID  && x.idType == type);

                    if (!boolThumbExists)
                    {
                        idThumbs newThumb = new idThumbs
                        {
                            GUID = Guid.NewGuid().ToString().ToUpper(),
                            ImageGUID = catalogItem.GUID,
                            idThumb = StreamToByteArray(resizedImageStream),
                            idType = type
                        };

                        dbThumbs.idThumbs.Add(newThumb);
                    }

                    result.ImageStreams.Add(resizedImageStream);
                }

                if (imageStream != null) { imageStream.Close(); }
                await dbThumbs.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (imageStream != null) { imageStream.Close(); }
                result.Exceptions.Add(new Exception(String.Format("Error generating thumbnail for imageGUID {0} file {1}", catalogItem.GUID, filePath), ex));
            }

            return result;
        }

        public async static Task<XDocument> GetRecipeXDocument(IDImagerDB db, idCatalogItem catalogItem)
        {
            if (catalogItem.idHasRecipe > 0)
            {
                idImageData imageData = await db.idImageData.SingleOrDefaultAsync(x => x.ImageGUID.Equals(catalogItem.GUID) && x.DataName.Equals("XMP"));

                if (imageData != null)
                {
                    MemoryStream compressedXmpStream = new MemoryStream(imageData.idData);
                    MemoryStream decompressedXmpStream = new MemoryStream();
                    ZOutputStream outZStream = new ZOutputStream(decompressedXmpStream);

                    compressedXmpStream.CopyTo(outZStream);
                    decompressedXmpStream.Position = 0;
                    XDocument xdocument = XDocument.Load(decompressedXmpStream);
                    compressedXmpStream.Close();
                    decompressedXmpStream.Close();

                    return xdocument;
                }
            }

            return null;
        }

        public static byte[] StreamToByteArray(Stream inputStream)
        {
            if (!inputStream.CanRead)
                throw new ArgumentException();

            if (inputStream.CanSeek)
                inputStream.Seek(0, SeekOrigin.Begin);

            byte[] output = new byte[inputStream.Length];
            int bytesRead = inputStream.Read(output, 0, output.Length);

            if (inputStream.CanSeek)
                inputStream.Seek(0, SeekOrigin.Begin);

            return output;
        }

        public static void Resize(MagickImage image, uint width, uint height)
        {
            if (image.Width > width && image.Height > height)
            {
                image.Resize(width, height);
            }
        }

        /// <summary>
        /// Transcodes video to different resolution and mp4
        /// </summary>
        /// <param name="ffmpegEngine">FFmpeg engine</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <param name="filePath">Video file path</param>
        /// <param name="transcodeFilePath">Transcode file path</param>
        /// <param name="transcodeVideoSize">Target video size</param>
        /// <param name="originalVideoWidth">Optional original video width</param>
        /// <param name="originalVideoHeight">Optional original video height</param>
        /// <returns>Transform task</returns>
        public async static Task TranscodeVideo(Engine ffmpegEngine, CancellationToken cancellationToken, string filePath, string transcodeFilePath,
            string transcodeVideoSize, int originalVideoWidth = 0, int originalVideoHeight = 0)
        {
            GetTranscodeVideoSize(transcodeVideoSize, originalVideoWidth, originalVideoHeight, out VideoSize targetVideoSize,
                out int targetVideoWidth, out int targetVideoHeight);
            var conversionOptions = GetConversionOptions(targetVideoSize, targetVideoWidth, targetVideoHeight);
            await TranscodeVideo(ffmpegEngine, cancellationToken, filePath, transcodeFilePath, conversionOptions);
        }

        /// <summary>
        /// Transcodes video to different resolution and mp4
        /// </summary>
        /// <param name="ffmpegEngine">FFmpeg engine</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <param name="filePath">Video file path</param>
        /// <param name="transcodeFilePath">Transcoded video file path</param>
        /// <param name="conversionOptions">ConversionOptions</param>
        /// <returns>Transform task</returns>
        public async static Task TranscodeVideo(Engine ffmpegEngine, CancellationToken cancellationToken, string filePath, string transcodeFilePath,
            ConversionOptions conversionOptions)
        {
            if (!File.Exists(transcodeFilePath))
            {
                var inputFile = new InputFile(filePath);
                var outputFile = new OutputFile(transcodeFilePath);

                FileInfo transcodeFileInfo = new(transcodeFilePath);

                if (!transcodeFileInfo.Directory.Exists)
                    transcodeFileInfo.Directory.Create();

                await ffmpegEngine.ConvertAsync(inputFile, outputFile, conversionOptions, cancellationToken);
            }
        }

        /// <summary>
        /// Returns transcode video file path
        /// </summary>
        /// <param name="guid">Photosupreme Guid of video</param>
        /// <param name="transcodeDirectory">Directory to store transcoded video</param>
        /// <param name="transcodeVideoSize">Target video size</param>
        /// <returns></returns>
        public static string GetTranscodeFilePath(string guid, string transcodeDirectory, string transcodeVideoSize)
        {
            string strTranscodeDirectory = Path.Combine(transcodeDirectory, transcodeVideoSize, guid.Substring(0, 2));
            return Path.Combine(strTranscodeDirectory, guid + ".mp4");
        }

        /// <summary>
        /// Returns FFmpeg Engine
        /// </summary>
        /// <returns>FFmpeg Engine</returns>
        public static Engine GetFFmpegEngine()
        {
            return new Engine(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg");
        }

        /// <summary>
        /// Returns FFmpeg conversion options
        /// </summary>
        /// <param name="videoSize">FFmpeg videosize</param>
        /// <param name="width">Video width</param>
        /// <param name="height">Vidoe height</param>
        /// <returns></returns>
        public static ConversionOptions GetConversionOptions(VideoSize videoSize, int width, int height)
        {
            if (videoSize == VideoSize.Custom)
            {
                return new ConversionOptions
                {
                    VideoSize = videoSize,
                    CustomWidth = width,
                    CustomHeight = height
                };
            }
            else
            {
                return new ConversionOptions
                {
                    VideoSize = videoSize
                };
            }
        }

        static double RoundToNearestEven(double value) =>
            Math.Truncate(value) + Math.Truncate(value) % 2;

        /// <summary>
        /// Calculates the resolution of the transcoded video
        /// </summary>
        /// <param name="transcodeVideoSize">FFmpeg videosize</param>
        /// <param name="originalVideoWidth">Original video width</param>
        /// <param name="originalVideoHeight">Original video height</param>
        /// <param name="videoSize">Returns used FFmpeg videosize</param>
        /// <param name="width">Returns video width</param>
        /// <param name="height">Returns video height</param>
        public static void GetTranscodeVideoSize(string transcodeVideoSize, int originalVideoWidth, int originalVideoHeight,
            out VideoSize videoSize, out int width, out int height)
        {
            videoSize = (VideoSize)Enum.Parse(typeof(VideoSize), transcodeVideoSize, true);

            if (originalVideoWidth > 0 && originalVideoHeight > 0)
            {
                int targetHeight;

                switch (videoSize)
                {
                    case VideoSize.Hd480:
                        targetHeight = 480;
                        break;
                    case VideoSize.Hd720:
                        targetHeight = 720;
                        break;
                    case VideoSize.Hd1080:
                        targetHeight = 1080;
                        break;
                    default:
                        throw new Exception(string.Format("Not supported transcode video size: {0}", transcodeVideoSize));
                }

                videoSize = VideoSize.Custom;

                if (originalVideoWidth < originalVideoHeight)
                {
                    // Vertical video, swap height and width
                    if (originalVideoWidth > targetHeight)
                    {
                        double dAspectRatio = (double)originalVideoHeight / originalVideoWidth;
                        width = targetHeight;
                        height = (int)RoundToNearestEven(targetHeight * dAspectRatio);
                    }
                    else
                    {
                        width = originalVideoHeight;
                        height = originalVideoWidth;
                    }
                }
                else
                {
                    if (originalVideoHeight > targetHeight)
                    {
                        double dAspectRatio = (double)originalVideoWidth / originalVideoHeight;
                        width = (int)RoundToNearestEven(targetHeight * dAspectRatio);
                        height = targetHeight;
                    }
                    else
                    {
                        width = originalVideoWidth;
                        height = originalVideoHeight;
                    }
                }
            }
            else
            {
                switch (videoSize)
                {
                    case VideoSize.Hd480:
                        width = 852;
                        height = 480;
                        break;
                    case VideoSize.Hd720:
                        width = 1280;
                        height = 720;
                        break;
                    case VideoSize.Hd1080:
                        width = 1920;
                        height = 1080;
                        break;
                    default:
                        throw new Exception(string.Format("Not supported transcode video size: {0}", transcodeVideoSize));
                }
            }
        }

        /// <summary>
        /// Sets db options on DbContextOptionsBuilder
        /// </summary>
        /// <param name="optionsBuilder">DbContextOptionsBuilder to set</param>
        /// <param name="dbType">DBType as string</param>
        /// <param name="connectionString">Connection string</param>
        public static void SetDbContextOptions(DbContextOptionsBuilder optionsBuilder, string dbType,
            string connectionString)
        {
            if (dbType.Equals("MsSql"))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
            else if (dbType.Equals("Postgres"))
            {
                optionsBuilder.UseNpgsql(connectionString);
                optionsBuilder.ReplaceService<ISqlGenerationHelper, PostgresSqlGenerationHelper>();
            }
            else
            {
                throw new Exception("DBType not supported, supported type are 'MsSql' and 'Postgres'.");
            }
        }

        /// <summary>
        /// Returns a BadRequestObjectResult for missing arguments.
        /// </summary>
        /// <param name="argumentName">Missing argument name</param>
        /// <returns>BadRequestObjectResult</returns>
        public static BadRequestObjectResult BadRequestArgumentNull([ActionResultObjectValue] object argumentName)
        {
            return new BadRequestObjectResult(string.Format("Missing argument: {0}", argumentName));
        }
    }
}
