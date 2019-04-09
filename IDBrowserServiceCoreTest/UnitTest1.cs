using IDBrowserServiceCore;
using IDBrowserServiceCore.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
            ImagePropertyGuids = new List<String>();

            ImageGuids = Controller.GetRandomImageGuids();

            List<ImageProperty> ImageProperties = Controller.GetImageProperties(null);
            foreach (ImageProperty imageProperty in ImageProperties)
            {
                ImagePropertyGuids.Add(imageProperty.GUID);
            }
        }

        private readonly IConfiguration configuration;
        public IConfiguration Configuration
        {
            get
            {
                if (configuration == null)
                    return new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .Build();

                return configuration;
            }
        }

        private readonly ILoggerFactory logger;
        public ILoggerFactory Logger
        {
            get
            {
                if (logger == null)
                    return new LoggerFactory()
                        .AddConsole()
                        .AddDebug();

                return logger;
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
    }
}
