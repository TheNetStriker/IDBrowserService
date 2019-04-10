using IDBrowserServiceCore;
using IDBrowserServiceCore.Controllers;
using IDBrowserServiceCore.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace IDBrowserServiceCoreTest
{
    public class UnitTest1
    {
        private static List<String> ImagePropertyGuids;
        private static List<String> ImageGuids;

        public UnitTest1()
        {
            StaticFunctions.Configuration = Configuration;
            ImagePropertyGuids = new List<String>();
            ImageGuids = Controller.GetRandomImageGuids();

            List<ImageProperty> ImageProperties = Controller.GetImageProperties(null);
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
                    configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .Build();

                return configuration;
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
                    db = new IDImagerDB(configuration["ConnectionStrings:IDImager"]);

                return db;
            }
        }

        private IDImagerDB dbThumbs;
        public IDImagerDB DbThumbs
        {
            get
            {
                if (dbThumbs == null)
                    dbThumbs = new IDImagerDB(configuration["ConnectionStrings:IDImagerThumbs"]);

                return dbThumbs;
            }
        }

        private readonly ValuesController controller;
        public ValuesController Controller
        {
            get
            {
                if (controller == null)
                    return new ValuesController(Configuration, Logger, null);

                return controller;
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
        public void GetImagePropertiesTest()
        {
            List<ImageProperty> list = Controller.GetImageProperties(null);
            list = Controller.GetImageProperties(list.First().GUID);
        }

        [Fact]
        public void GetImagePropertyThumbnailTest()
        {
            String imagePropertyGuid = GetNextImagePropertyGuid();
            Controller.GetImagePropertyThumbnail(imagePropertyGuid, "true");
        }

        [Fact]
        public void GetCatalogItemsTest()
        {
            Controller.GetCatalogItems("true", GetNextImagePropertyGuid());
        }

        [Fact]
        public void GetImageThumbnailTest()
        {
            Stream stream = Controller.GetImageThumbnail("T", GetNextImageGuid());
            stream.Close();
            stream = Controller.GetImageThumbnail("M", GetNextImageGuid());
            stream.Close();
        }

        [Fact]
        public void GetImageTest()
        {
            Stream stream = Controller.GetImage(GetNextImageGuid());
            stream.Close();
        }

        [Fact]
        public void GetResizedImageTest()
        {
            Stream stream = Controller.GetResizedImage("640", "480", GetNextImageGuid());
            stream.Close();
        }

        [Fact]
        public void GetImageInfoTest()
        {
            Controller.GetImageInfo(GetNextImageGuid());
        }

        [Fact]
        public void SearchImagePropertiesTest()
        {
            List<ImagePropertyRecursive> result = Controller.SearchImageProperties("David Masshardt");
            if (result.Count == 0)
                throw new Exception("No items found with SearchImagePropertiesSoap");
        }

        [Fact]
        public void GetCatalogItemImagePropertiesTest()
        {
            List<ImagePropertyRecursive> result = Controller.GetCatalogItemImageProperties(GetNextImageGuid());
            if (result.Count == 0)
                throw new Exception("No image properties found with GetCatalogItemImagePropertiesSoap");
        }

        [Fact]
        public void GetFilePathsTest()
        {
            List<FilePath> result = Controller.GetFilePaths();
            if (result.Count == 0)
                throw new Exception("No file paths found with GetFilePathsSoap");
        }

        [Fact]
        public void SaveImageThumbnailTest()
        {
            Boolean keepAspectRatio = Boolean.Parse(Configuration["IDBrowserServiceSettings:KeepAspectRatio"]);
            Boolean setGenericVideoThumbnailOnError = Boolean.Parse(Configuration["IDBrowserServiceSettings:SetGenericVideoThumbnailOnError"]);
            idCatalogItem idCatalogItem = Db.idCatalogItem.Include("idFilePath").First();
            List<String> types = new List<String>() { "T", "R", "M" };

            SaveImageThumbnailResult result = StaticFunctions
                .SaveImageThumbnail(idCatalogItem, Db, DbThumbs, types, keepAspectRatio, setGenericVideoThumbnailOnError);

            if (result.Exceptions.Count > 0)
                throw result.Exceptions.First();
        }

        [Fact]
        public void SaveVideoThumbnailTest()
        {
            Boolean keepAspectRatio = Boolean.Parse(Configuration["IDBrowserServiceSettings:KeepAspectRatio"]);
            Boolean setGenericVideoThumbnailOnError = Boolean.Parse(Configuration["IDBrowserServiceSettings:SetGenericVideoThumbnailOnError"]);
            idCatalogItem idCatalogItem = Db.idCatalogItem.Include("idFilePath").Where(x => x.FileName.EndsWith(".MP4")).First();
            List<String> types = new List<String>() { "T", "R", "M" };

            SaveImageThumbnailResult result = StaticFunctions
                .SaveImageThumbnail(idCatalogItem, Db, DbThumbs, types, keepAspectRatio, setGenericVideoThumbnailOnError);

            if (result.Exceptions.Count > 0)
                throw result.Exceptions.First();
        }
    }
}
