using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using IDBrowserServiceTest.IDBrowserService;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Net;
using System.Net.Security;

namespace IDBrowserServiceTest
{
    [TestClass]
    public class UnitTest1
    {
        private static List<String> ImagePropertyGuids;
        private static List<String> ImageGuids;

        [ClassInitialize]
        public static void Initilization(TestContext context)
        {
            ImagePropertyGuids = new List<String>();

            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);

            SoapServiceClient service = new SoapServiceClient();
            ImageGuids = service.GetRandomImageGuidsSoap();

            List<IDBrowserServiceCode.ImageProperty> ImageProperties = service.GetImagePropertiesSoap(null);
            foreach (IDBrowserServiceCode.ImageProperty imageProperty in ImageProperties)
            {
                ImagePropertyGuids.Add(imageProperty.GUID);
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

        [TestMethod]
        public void XmpRecipeTest()
        {
            System.IO.Stream imageStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("IDBrowserServiceTest.Resources.Example.png");

            System.IO.Stream xmpStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("IDBrowserServiceTest.Resources.XMPTest.xml");
            System.Xml.Linq.XDocument xdocument = System.Xml.Linq.XDocument.Load(xmpStream);

            IDBrowserServiceCode.Recipe.ApplyXmpRecipe(xdocument, imageStream, "PNG");
        }

        [TestMethod]
        public void GetImagePropertiesTest()
        {
            SoapServiceClient service = new SoapServiceClient();
            service.GetImagePropertiesSoap(null);
        }

        [TestMethod]
        public void GetImagePropertyThumbnail()
        {
            SoapServiceClient service = new SoapServiceClient();
            String imagePropertyGuid = GetNextImagePropertyGuid();
            service.GetImagePropertyThumbnailSoap(imagePropertyGuid, "true");
        }

        [TestMethod]
        public void GetCatalogItemsTest()
        {
            SoapServiceClient service = new SoapServiceClient();
            service.GetCatalogItemsSoap("true", GetNextImagePropertyGuid());
        }

        [TestMethod]
        public void GetImageThumbnailTest()
        {
            SoapServiceClient service = new SoapServiceClient();
            System.IO.Stream stream = service.GetImageThumbnailSoap("T", GetNextImageGuid());
            stream.Close();
            stream = service.GetImageThumbnailSoap("M", GetNextImageGuid());
            stream.Close();
        }

        [TestMethod]
        public void GetImageTest()
        {
            SoapServiceClient service = new SoapServiceClient();
            System.IO.Stream stream = service.GetImageSoap(GetNextImageGuid());
            stream.Close();
        }

        [TestMethod]
        public void GetResizedImageTest()
        {
            SoapServiceClient service = new SoapServiceClient();
            System.IO.Stream stream = service.GetResizedImageSoap("640", "480", GetNextImageGuid());
            stream.Close();
        }

        [TestMethod]
        public void GetImageInfoTest()
        {
            SoapServiceClient service = new SoapServiceClient();
            service.GetImageInfoSoap(GetNextImageGuid());
        }

        [TestMethod]
        public async Task RestTest()
        {
            SoapServiceClient service = new SoapServiceClient();
            Uri uri = new Uri(service.Endpoint.Address.Uri, "GetImageProperties");
            Task<string> test = GetAsync(uri.ToString());
            string result = await test;
            Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert.Contains(result, "<ArrayOfImageProperty");
        }

        [TestMethod]
        public async Task SearchImagePropertiesTest()
        {
            SoapServiceClient service = new SoapServiceClient();
            List<IDBrowserServiceCode.ImagePropertyRecursive> result = service.SearchImagePropertiesSoap("David Masshardt");
            if (result.Count == 0)
                throw new Exception("No items found with SearchImagePropertiesSoap");
        }

        [TestMethod]
        public async Task GetCatalogItemImagePropertiesTest()
        {
            SoapServiceClient service = new SoapServiceClient();
            List<IDBrowserServiceCode.ImagePropertyRecursive>  result = service.GetCatalogItemImagePropertiesSoap(GetNextImageGuid());
            if (result.Count == 0)
                throw new Exception("No image properties found with GetCatalogItemImagePropertiesSoap");
        }

        //[TestMethod]
        //public async Task AddCatalogItemDefinitionTest()
        //{
            
        //}

        //[TestMethod]
        //public async Task DeleteCatalogItemDefinitionTest()
        //{
            
        //}

        //[TestMethod]
        //public async Task AddImagePropertyTest()
        //{
            
        //}

        //[TestMethod]
        //public async Task GetFileTest()
        //{
            
        //}

        [TestMethod]
        public async Task GetFilePathsTest()
        {
            SoapServiceClient service = new SoapServiceClient();
            List<IDBrowserServiceCode.FilePath> result = service.GetFilePathsSoap();
            if (result.Count == 0)
                throw new Exception("No file paths found with GetFilePathsSoap");
        }

        private async Task<string> GetAsync(string uri)
        {
            try
            {
                //X509Certificate2 certificate = CertificateUtil.GetCertificate(StoreName.My, StoreLocation.LocalMachine, "CN=IDBrowserClient");
                //var certificate = new X509Certificate2();
                WebRequestHandler handler = new WebRequestHandler();

                ServicePointManager.ServerCertificateValidationCallback +=
                    new RemoteCertificateValidationCallback((sender, certificate2, chain, policyErrors) => { return true; });
                //handler.ClientCertificates.Add(certificate);

                HttpClient client = new HttpClient(handler);

                var response = await client.GetAsync(new Uri(uri));
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
