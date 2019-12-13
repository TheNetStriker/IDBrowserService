using ComponentAce.Compression.Libs.zlib;
using IDBrowserServiceCore.Data;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using ImageMagick;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using IDBrowserServiceCore.Code.XmpRecipe;
using IDBrowserServiceCore.Settings;
using FFmpeg.NET;
using FFmpeg.NET.Enums;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Storage;
using IDBrowserServiceCore.Data.PostgresHelpers;

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
            string strFilePath = catalogItem.idFilePath.FilePath;

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
                    int imageWidth;
                    int imageHeight;

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
                        magickReadSettings = new MagickReadSettings { Format = magickFormat };
                    }

                    imageStream.Position = 0;

                    MagickImage image = new MagickImage(imageStream, magickReadSettings)
                    {
                        Format = MagickFormat.Jpeg,
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

        public static void Resize(MagickImage image, int width, int height)
        {
            if (image.Width > width && image.Height > height)
            {
                image.Resize(width, height);
            }
        }

        public async static Task<string> TranscodeVideo(string filePath, string guid, string transcodeDirectory, string videosize)
        {
            string strTranscodeDirectory = Path.Combine(transcodeDirectory, videosize, guid.Substring(0, 2));
            string strTranscodeFilePath = Path.Combine(strTranscodeDirectory, guid + ".mp4");

            if (!File.Exists(strTranscodeFilePath))
            {
                var inputFile = new MediaFile(filePath);
                var outputFile = new MediaFile(strTranscodeFilePath);
                VideoSize videoSize = (VideoSize)Enum.Parse(typeof(VideoSize), videosize);

                if (!Directory.Exists(strTranscodeDirectory))
                    Directory.CreateDirectory(strTranscodeDirectory);

                var conversionOptions = new ConversionOptions
                {
                    VideoSize = videoSize
                };

                var ffmpeg = new Engine(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg");
                await ffmpeg.ConvertAsync(inputFile, outputFile, conversionOptions);
            }

            return strTranscodeFilePath;
        }

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

        public static DbContextOptionsBuilder<TContext> GetIDImagerDBOptionsBuilder<TContext>(string dbType,
            string connectionString) where TContext : DbContext
        {
            DbContextOptionsBuilder<TContext> optionsBuilder = new DbContextOptionsBuilder<TContext>();
            SetDbContextOptions(optionsBuilder, dbType, connectionString);
            return optionsBuilder;
        }

        /// <summary>
        /// Transcode's all videos of a site
        /// </summary>
        /// <param name="configuration">Json configuration of IDBrowserService as IDBrowserConfiguration class</param>
        /// <param name="siteName">Site name to transcode</param>
        /// <param name="videoSize">Video size to transcode. (e.g. Hd480, Hd720, Hd1080)</param>
        public static void TranscodeAllVideos(IDBrowserConfiguration configuration, string siteName, string videoSize)
        {
            SiteSettings siteSettings = configuration.Sites[siteName];

            IDImagerDB db = new IDImagerDB(GetIDImagerDBOptionsBuilder<IDImagerDB>(siteSettings.ConnectionStrings.DBType,
                siteSettings.ConnectionStrings.IDImager).Options);

            var query = db.idCatalogItem
                .Include(x => x.idFilePath)
                .Where(x => configuration.VideoFileExtensions.Contains(x.idFileType));

            int intTotalCount = query.Count();
            int intCounter = 0;

            foreach (idCatalogItem catalogItem in query)
            {
                string strFilePath = StaticFunctions.GetImageFilePath(catalogItem, siteSettings.ServiceSettings.FilePathReplace);
                string strTranscodedFile = StaticFunctions.TranscodeVideo(strFilePath, catalogItem.GUID,
                    siteSettings.ServiceSettings.TranscodeDirectory, videoSize).Result;

                intCounter++;

                Console.WriteLine(string.Format("{0}/{1}: {2}", intCounter, intTotalCount, strFilePath));
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
                        SaveImageThumbnailResult result = await SaveImageThumbnail(catalogItem, db, dbThumbs, typesToGenerate, 
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
