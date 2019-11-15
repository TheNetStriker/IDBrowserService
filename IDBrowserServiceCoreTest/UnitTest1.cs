using IDBrowserServiceCore;
using IDBrowserServiceCore.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IDBrowserServiceCore.Settings;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Storage;
using IDBrowserServiceCore.Data.PostgresHelpers;

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
            ImageGuids = Task.Run(() => Controller.GetRandomImageGuids(imageFileExtensions)).Result.Value;

            idCatalogItemFirstImage = Db.idCatalogItem.Include(x => x.idFilePath).Where(x => x.FileName.EndsWith(".JPG")).First();
            idCatalogItemFirstVideo = Db.idCatalogItem.Include(x => x.idFilePath).Where(x => x.FileName.EndsWith(".MP4")).First();
            idPropFirst = db.idProp.First();

            List<ImageProperty> ImageProperties = Task.Run(() => Controller.GetImageProperties(null)).Result.Value;
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
                return "Server=172.17.2.23;Database=photosupreme;user id=idimager_main;password=idi_main_2606;";
            }
        }

        public string DbThumbsConnectionString
        {
            get
            {
                return "Server=172.17.2.23;Database=photosupreme_thumbs;user id=idimager_main;password=idi_main_2606;";
            }
        }

        private ILoggerFactory logger;
        public ILoggerFactory Logger
        {
            get
            {
                if (logger == null)
                    logger = new LoggerFactory()
                        .AddConsole()
                        .AddDebug();

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
                if (dbThumbs == null)
                {
                    var options = SqlServerDbContextOptionsExtensions
                        .UseSqlServer(new DbContextOptionsBuilder<IDImagerThumbsDB>(), DbThumbsConnectionString).Options;
                    dbThumbs = new IDImagerThumbsDB(options);
                }

                return dbThumbs;
            }
        }

        private ValuesController controller;
        public ValuesController Controller
        {
            get
            {
                if (controller == null)
                    controller = new ValuesController(Db, DbThumbs, Options.Create<ServiceSettings>(Settings), Logger);

                return controller;
            }
        }

        private MediaController mediaController;
        public MediaController MediaController
        {
            get
            {
                if (mediaController == null)
                    mediaController = new MediaController(Db, Options.Create<ServiceSettings>(Settings), Logger);

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
            ActionResult<List<ImageProperty>> result = await Controller.GetImageProperties(null);
            result = await Controller.GetImageProperties(result.Value.First().GUID);
        }

        [Fact]
        public async void GetImagePropertyThumbnailTest()
        {
            String imagePropertyGuid = GetNextImagePropertyGuid();
            await Controller.GetImagePropertyThumbnail(imagePropertyGuid, "true");
        }

        [Fact]
        public async void GetCatalogItemsTest()
        {
            await Controller.GetCatalogItems("true", GetNextImagePropertyGuid());
        }

        [Fact]
        public async void GetImageThumbnailTest()
        {
            ActionResult<Stream> result = await Controller.GetImageThumbnail("T", GetNextImageGuid());
            result.Value.Close();
            result = await Controller.GetImageThumbnail("M", GetNextImageGuid());
            result.Value.Close();
        }

        [Fact]
        public async void GetImageTest()
        {
            ActionResult<Stream> result = await Controller.GetImage(ReceipeTestGuid);
            result.Value.Close();
        }

        [Fact]
        public async void GetResizedImageTest()
        {
            ActionResult<Stream> result = await Controller.GetResizedImage("640", "480", GetNextImageGuid());
            result.Value.Close();
        }

        [Fact]
        public async void GetImageInfoTest()
        {
            await Controller.GetImageInfo(GetNextImageGuid());
        }

        [Fact]
        public async void SearchImagePropertiesTest()
        {
            ActionResult<List<ImagePropertyRecursive>> result = await Controller.SearchImageProperties("David Masshardt");
            if (result.Value.Count == 0)
                throw new Exception("No items found with SearchImagePropertiesSoap");
        }

        [Fact]
        public async void GetCatalogItemImagePropertiesTest()
        {
            ActionResult<List<ImagePropertyRecursive>> result = await Controller.GetCatalogItemImageProperties(GetNextImageGuid());
            if (result.Value.Count == 0)
                throw new Exception("No image properties found with GetCatalogItemImagePropertiesSoap");
        }

        [Fact]
        public async void GetFilePathsTest()
        {
            ActionResult<List<FilePath>> result = await Controller.GetFilePaths();
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
            await Controller.AddCatalogItemDefinition(idPropFirst.GUID, idCatalogItemFirstImage.GUID);
        }

        [Fact]
        public async void DeleteCatalogItemDefinitionTest()
        {
            await Controller.DeleteCatalogItemDefinition(idPropFirst.GUID, idCatalogItemFirstImage.GUID);
        }

        [Fact]
        public async void MediaControllerPlayTest()
        {
            if (!(await MediaController.Play(idCatalogItemFirstVideo.GUID) is FileStreamResult stream))
                throw new Exception("No stream received");
            else
                stream.FileStream.Close();
        }
    }
}
