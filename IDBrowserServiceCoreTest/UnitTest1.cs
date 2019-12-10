using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Controllers;
using IDBrowserServiceCore.Data;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using IDBrowserServiceCore.Data.PostgresHelpers;
using IDBrowserServiceCore.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Hosting;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IDBrowserServiceCoreTest
{
    public class UnitTest1
    {
        private const string ReceipeTestGuid = "675BB1838073452BA98DF29E0D5249DD"; // "607F77D8CE8048A798C68336051E5CFC";
        private const string DBType = "Postgres";

        private static List<String> ImagePropertyGuids;
        private static List<String> ImageGuids;

        idCatalogItem idCatalogItemFirstImage;
        idCatalogItem idCatalogItemFirstVideo;
        idProp idPropFirst;

        public UnitTest1()
        {
            ImagePropertyGuids = new List<String>();
            List<string> imageFileExtensions = new List<string>() { "JPG", "JPEG", "TIF", "PNG", "GIF", "BMP" };
            ImageGuids = Task.Run(() => ValuesController.GetRandomImageGuids(imageFileExtensions)).Result.Value;

            idCatalogItemFirstImage = Db.idCatalogItem
                .Include(x => x.idFilePath)
                .Where(x => x.FileName.EndsWith(".JPG"))
                .OrderBy(x => x.DateTimeStamp)
                .First();
            idCatalogItemFirstVideo = Db.idCatalogItem
                .Include(x => x.idFilePath)
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

        private ServiceSettings settings;
        public ServiceSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = new ServiceSettings()
                    {
                        CreateThumbnails = true,
                        MThumbmailWidth = 1680,
                        MThumbnailHeight = 1260,
                        KeepAspectRatio = true,
                        SetGenericVideoThumbnailOnError = true,
                    };

                    FilePathReplaceSettings filePathReplaceSettings = new FilePathReplaceSettings()
                    {
                        PathMatch = "\\\\QNAPNAS01\\Multimedia",
                        PathReplace = "\\\\172.17.2.14\\Multimedia"
                    };

                    settings.FilePathReplace.Add(filePathReplaceSettings);
                }

                return settings;
            }
        }

        public string DbConnectionString
        {
            get
            {
                return "Host=172.17.2.17;Database=photosupreme_mad;Username=idimager_main;Password=idi_main_2606;";
            }
        }

        public string DbThumbsConnectionString
        {
            get
            {
                return "Host=172.17.2.17;Database=photosupreme_mad_thumbs;Username=idimager_main;Password=idi_main_2606;";
            }
        }

        private Logger logger;
        public Logger Logger
        {
            get
            {
                if (logger == null)
                    logger = new LoggerConfiguration()
                        .WriteTo.Console()
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
                        var options = NpgsqlDbContextOptionsExtensions
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
                    var options = NpgsqlDbContextOptionsExtensions
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
                       .AddSingleton<IOptions<ServiceSettings>>(Options.Create<ServiceSettings>(Settings))
                       .AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory(Logger, false))
                       .AddSingleton<ILogger<ValuesController>>(loggerFactory.CreateLogger<ValuesController>())
                       .AddSingleton<ILogger<MediaController>>(loggerFactory.CreateLogger<MediaController>())
                       .AddSingleton(diagnosticContext)
                       .AddSingleton<IDiagnosticContext>(diagnosticContext)
                       .AddSingleton<IConfiguration>(Configuration)
                       .AddTransient<ValuesController, ValuesController>()
                       .AddTransient<MediaController, MediaController>();

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
            result.Value.Close();
            result = await ValuesController.GetImageThumbnail("M", GetNextImageGuid());
            result.Value.Close();
        }

        [Fact]
        public async void GetImageTest()
        {
            ActionResult<Stream> result = await ValuesController.GetImage(ReceipeTestGuid);
            result.Value.Close();
        }

        [Fact]
        public async void GetResizedImageTest()
        {
            ActionResult<Stream> result = await ValuesController.GetResizedImage("640", "480", GetNextImageGuid());
            result.Value.Close();
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
            Boolean keepAspectRatio = Settings.KeepAspectRatio;
            Boolean setGenericVideoThumbnailOnError = Settings.SetGenericVideoThumbnailOnError;
            List<String> types = new List<String>() { "T", "R", "M" };

            SaveImageThumbnailResult result = await StaticFunctions
                .SaveImageThumbnail(idCatalogItemFirstImage, Db, DbThumbs, types, keepAspectRatio,
                setGenericVideoThumbnailOnError, Settings);

            if (result.Exceptions.Count > 0)
                throw result.Exceptions.First();
        }

        [Fact]
        public async void SaveVideoThumbnailTest()
        {
            Boolean keepAspectRatio = Settings.KeepAspectRatio;
            Boolean setGenericVideoThumbnailOnError = Settings.SetGenericVideoThumbnailOnError;
            
            List<String> types = new List<String>() { "T", "R", "M" };

            SaveImageThumbnailResult result = await StaticFunctions
                .SaveImageThumbnail(idCatalogItemFirstVideo, Db, DbThumbs, types, keepAspectRatio,
                setGenericVideoThumbnailOnError, Settings);

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
    }
}
