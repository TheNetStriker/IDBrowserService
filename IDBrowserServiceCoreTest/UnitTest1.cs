using FFmpeg.NET;
using FFmpeg.NET.Enums;
using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Controllers;
using IDBrowserServiceCore.Data;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using IDBrowserServiceCore.Data.PostgresHelpers;
using IDBrowserServiceCore.Services;
using IDBrowserServiceCore.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IDBrowserServiceCoreTest
{
    public class UnitTest1
    {
        
        private const string ReceipeTestGuid = "675BB1838073452BA98DF29E0D5249DD"; // "607F77D8CE8048A798C68336051E5CFC";
        private const string DBType = "Postgres";

        private static List<string> imageFileExtensions;
        private static List<String> ImagePropertyGuids;
        private static List<String> ImageGuids;

        idCatalogItem idCatalogItemFirstImage;
        idCatalogItem idCatalogItemFirstVideo;
        idProp idPropFirst;

        public UnitTest1()
        {
            ImagePropertyGuids = new List<String>();
            imageFileExtensions = new List<string>() { "JPG", "JPEG", "TIF", "PNG", "GIF", "BMP" };
            ImageGuids = Task.Run(() => ValuesController.GetRandomImageGuids(imageFileExtensions, 20)).Result.Value;

            idCatalogItemFirstImage = Db.idCatalogItem
                .Include(x => x.idFilePath)
                .Include(x => x.idCache_FilePath)
                .Where(x => x.FileName.EndsWith(".JPG"))
                .OrderBy(x => x.DateTimeStamp)
                .First();
            idCatalogItemFirstVideo = Db.idCatalogItem
                .Include(x => x.idFilePath)
                .Include(x => x.idCache_FilePath)
                .Where(x => x.FileName.EndsWith(".MP4"))
                .OrderBy(x => x.DateTimeStamp)
                .First();
            idPropFirst = Db.idProp.First();

            List<ImageProperty> ImageProperties = Task.Run(() => ValuesController.GetImageProperties(null)).Result.Value;
            foreach (ImageProperty imageProperty in ImageProperties)
            {
                ImagePropertyGuids.Add(imageProperty.GUID);
            }
        }

        private IConfiguration configuration;
        public IConfiguration Configuration
        {
            get
            {
                if (configuration == null)
                {
                    configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile("sites.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();
                }

                return configuration;
            }
        }

        private IDBrowserConfiguration idBrowserConfiguration;
        public IDBrowserConfiguration IDBrowserConfiguration
        {
            get
            {
                if (idBrowserConfiguration == null)
                {
                    idBrowserConfiguration = new IDBrowserConfiguration();
                    Configuration.Bind(idBrowserConfiguration);
                }

                return idBrowserConfiguration;
            }
        }

        public ServiceSettings Settings
        {
            get
            {
                return IDBrowserConfiguration.Sites.First().Value.ServiceSettings;
            }
        }

        public string DbConnectionString
        {
            get
            {
                return IDBrowserConfiguration.Sites.First().Value.ConnectionStrings.IDImager;
            }
        }

        public string DbThumbsConnectionString
        {
            get
            {
                return IDBrowserConfiguration.Sites.First().Value.ConnectionStrings.IDImagerThumbs;
            }
        }

        private Logger logger;
        public Logger Logger
        {
            get
            {
                if (logger == null)
                    logger = new LoggerConfiguration()
                        .CreateLogger();

                return logger;
            }
        }

        private IDImagerDB db;
        public IDImagerDB Db
        {
            get
            {
                if (db == null)
                {
                    if (DBType.Equals("MsSql"))
                    {
                        var options = SqlServerDbContextOptionsExtensions
                           .UseSqlServer(new DbContextOptionsBuilder<IDImagerDB>(), DbConnectionString)
                           .Options;
                        db = new IDImagerDB(options);
                    }
                    else if (DBType.Equals("Postgres"))
                    {
                        var options = NpgsqlDbContextOptionsBuilderExtensions
                            .UseNpgsql(new DbContextOptionsBuilder<IDImagerDB>(), DbConnectionString)
                            .ReplaceService<ISqlGenerationHelper, PostgresSqlGenerationHelper>()
                            .Options;
                        db = new IDImagerDB(options);
                    }
                    else
                    {
                        throw new Exception("DBType not supported, supported type are 'MsSql' and 'Postgres'.");
                    }
                }

                return db;
            }
        }

        private IDImagerThumbsDB dbThumbs;
        public IDImagerThumbsDB DbThumbs
        {
            get
            {
                if (DBType.Equals("MsSql"))
                {
                    var options = SqlServerDbContextOptionsExtensions
                       .UseSqlServer(new DbContextOptionsBuilder<IDImagerThumbsDB>(), DbThumbsConnectionString)
                       .Options;
                    dbThumbs = new IDImagerThumbsDB(options);
                }
                else if (DBType.Equals("Postgres"))
                {
                    var options = NpgsqlDbContextOptionsBuilderExtensions
                        .UseNpgsql(new DbContextOptionsBuilder<IDImagerThumbsDB>(), DbThumbsConnectionString)
                        .ReplaceService<ISqlGenerationHelper, PostgresSqlGenerationHelper>()
                        .Options;
                    dbThumbs = new IDImagerThumbsDB(options);
                }
                else
                {
                    throw new Exception("DBType not supported, supported type are 'MsSql' and 'Postgres'.");
                }

                return dbThumbs;
            }
        }

        private ServiceProvider serviceProvider;
        public ServiceProvider ServiceProvider
        {
            get
            {
                if (serviceProvider == null)
                {
                    var diagnosticContext = new DiagnosticContext(Logger);
                    var loggerFactory = new LoggerFactory();

                    IConfiguration Configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();

                    var serviceCollection = new ServiceCollection()
                       .AddDbContextPool<IDImagerDB>(options => options.UseNpgsql(DbConnectionString)
                           .ReplaceService<ISqlGenerationHelper, PostgresSqlGenerationHelper>())
                       .AddDbContextPool<IDImagerThumbsDB>(options => options.UseNpgsql(DbThumbsConnectionString)
                           .ReplaceService<ISqlGenerationHelper, PostgresSqlGenerationHelper>())
                       .AddSingleton<ServiceSettings>(Settings)
                       .AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory(Logger, false))
                       .AddSingleton<ILogger<ValuesController>>(loggerFactory.CreateLogger<ValuesController>())
                       .AddSingleton<ILogger<MediaController>>(loggerFactory.CreateLogger<MediaController>())
                       .AddSingleton(diagnosticContext)
                       .AddSingleton<IDiagnosticContext>(diagnosticContext)
                       .AddTransient<ValuesController, ValuesController>()
                       .AddTransient<MediaController, MediaController>()
                       .AddSingleton<IDatabaseCache, DatabaseCache>();

                    serviceProvider = serviceCollection.BuildServiceProvider();
                }

                return serviceProvider;
            }
        }

        private ValuesController valuesController;
        public ValuesController ValuesController
        {
            get
            {
                if (valuesController == null)
                    valuesController = ServiceProvider.GetService<ValuesController>();

                return valuesController;
            }
        }

        private MediaController mediaController;
        public MediaController MediaController
        {
            get
            {
                if (mediaController == null)
                    mediaController = ServiceProvider.GetService<MediaController>();

                return mediaController;
            }
        }

        private IDatabaseCache databaseCache;
        public IDatabaseCache DatabaseCache
        {
            get
            {
                if (databaseCache == null)
                    databaseCache = ServiceProvider.GetService<IDatabaseCache>();

                return databaseCache;
            }
        }

        private String GetNextImagePropertyGuid()
        {
            var rand = new Random();
            lock (ImagePropertyGuids)
            {
                return ImagePropertyGuids[rand.Next(ImagePropertyGuids.Count)];
            }
        }

        private String GetNextImageGuid()
        {
            var rand = new Random();
            lock (ImagePropertyGuids)
            {
                return ImageGuids[rand.Next(ImageGuids.Count)];
            }

        }

        [Fact]
        public async void GetImagePropertiesTest()
        {
            ActionResult<List<ImageProperty>> result = await ValuesController.GetImageProperties(null);
            result = await ValuesController.GetImageProperties(result.Value.First().GUID);
        }

        [Fact]
        public async void GetImagePropertyThumbnailTest()
        {
            String imagePropertyGuid = GetNextImagePropertyGuid();
            await ValuesController.GetImagePropertyThumbnail(imagePropertyGuid, "true");
        }

        [Fact]
        public async void GetCatalogItemsTest()
        {
            await ValuesController.GetCatalogItems("true", GetNextImagePropertyGuid());
        }

        [Fact]
        public async void GetImageThumbnailTest()
        {
            ActionResult<Stream> result = await ValuesController.GetImageThumbnail("T", GetNextImageGuid());
            if (!(result.Result is FileStreamResult stream1))
                throw new Exception("No stream received");
            else
                stream1.FileStream.Close();
            result = await ValuesController.GetImageThumbnail("M", GetNextImageGuid());
            if (!(result.Result is FileStreamResult stream2))
                throw new Exception("No stream received");
            else
                stream2.FileStream.Close();
        }

        [Fact]
        public async void GetImageTest()
        {
            foreach (string guid in ImageGuids)
            {
                ActionResult<Stream> result = await ValuesController.GetImage(guid);
                if (!(result.Result is FileStreamResult stream))
                    throw new Exception("No stream received");
                else
                    stream.FileStream.Close();
            }
        }

        [Fact]
        public async void GetResizedImageTest()
        {
            foreach (string guid in ImageGuids)
            {
                ActionResult<Stream> result = await ValuesController.GetResizedImage("640", "480", guid);
                if (!(result.Result is FileStreamResult stream))
                    throw new Exception("No stream received");
                else
                    stream.FileStream.Close();
            }
        }

        [Fact]
        public async void GetImageInfoTest()
        {
            await ValuesController.GetImageInfo(GetNextImageGuid());
        }

        [Fact]
        public async void SearchImagePropertiesTest()
        {
            ActionResult<List<ImagePropertyRecursive>> result = await ValuesController.SearchImageProperties("David Masshardt");
            if (result.Value.Count == 0)
                throw new Exception("No items found with SearchImagePropertiesSoap");
        }

        [Fact]
        public async void GetCatalogItemImagePropertiesTest()
        {
            ActionResult<List<ImagePropertyRecursive>> result = await ValuesController.GetCatalogItemImageProperties(GetNextImageGuid());
            if (result.Value.Count == 0)
                throw new Exception("No image properties found with GetCatalogItemImagePropertiesSoap");
        }

        [Fact]
        public async void GetFilePathsTest()
        {
            ActionResult<List<FilePath>> result = await ValuesController.GetFilePaths();
            if (result.Value.Count == 0)
                throw new Exception("No file paths found with GetFilePathsSoap");
        }

        [Fact]
        public async void SaveImageThumbnailTest()
        {
            List<string> types = new List<string>() { "T", "R", "M" };

            SaveImageThumbnailResult result = await StaticFunctions
                .SaveImageThumbnail(idCatalogItemFirstImage, Db, DbThumbs, types, Settings);

            if (result.Exceptions.Count > 0)
                throw result.Exceptions.First();
        }

        [Fact]
        public async void SaveVideoThumbnailTest()
        {         
            List<string> types = new List<string>() { "T", "R", "M" };

            SaveImageThumbnailResult result = await StaticFunctions
                .SaveImageThumbnail(idCatalogItemFirstVideo, Db, DbThumbs, types, Settings);

            if (result.Exceptions.Count > 0)
                throw result.Exceptions.First();
        }

        [Fact]
        public async void AddCatalogItemDefinitionTest()
        {
            await ValuesController.AddCatalogItemDefinition(idPropFirst.GUID, idCatalogItemFirstImage.GUID);
        }

        [Fact]
        public async void DeleteCatalogItemDefinitionTest()
        {
            await ValuesController.DeleteCatalogItemDefinition(idPropFirst.GUID, idCatalogItemFirstImage.GUID);
        }

        [Fact]
        public async void MediaControllerPlayTest()
        {
            if (!(await MediaController.Play(idCatalogItemFirstVideo.GUID, null) is FileStreamResult stream))
                throw new Exception("No stream received");
            else
                stream.FileStream.Close();
        }

        [Fact]
        public async void MediaControllerPlayTranscodeTest()
        {
            if (!(await MediaController.Play(idCatalogItemFirstVideo.GUID, "Hd480") is FileStreamResult stream))
                throw new Exception("No stream received");
            else
                stream.FileStream.Close();
        }

        [Fact]
        public async void DatabaseCacheTest()
        {
            await DatabaseCache.CheckAndUpdateCacheAsync();
        }

        [Fact]
        public async void VideoTranscodeTest()
        {
            string videoSize = "Hd480";

            idCatalogItemFirstVideo.GetHeightAndWidth(out int originalVideoWidth, out int originalVideoHeight);

            Engine ffmpegEngine = StaticFunctions.GetFFmpegEngine();

            ffmpegEngine.Error += (sender, eventArgs) =>
            {
                Assert.Fail(eventArgs.Exception.ToString());
            };

            string strTranscodeFilePath = StaticFunctions.GetTranscodeFilePath(idCatalogItemFirstVideo.GUID,
                Settings.TranscodeDirectory, videoSize);

            FileInfo transcodeFileInfo = new FileInfo(strTranscodeFilePath);

            if (transcodeFileInfo.Exists)
            {
                transcodeFileInfo.Delete();
            }

            string strFilePath = StaticFunctions.GetImageFilePath(idCatalogItemFirstVideo, Settings.FilePathReplace);

            await StaticFunctions.TranscodeVideo(ffmpegEngine, default, strFilePath, strTranscodeFilePath,
                videoSize, originalVideoWidth, originalVideoHeight);

            transcodeFileInfo.Refresh();

            if (!transcodeFileInfo.Exists)
            {
                Assert.Fail("Transcoding failed, file does not exist.");
            }
        }
    }
}
