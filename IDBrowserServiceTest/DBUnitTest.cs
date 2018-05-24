using IDBrowserServiceCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDBrowserServiceTest
{
    [TestClass]
    public class DBUnitTest
    {
        [TestMethod]
        public void DBTest1()
        {
            Service service = new Service();

            List<ImageProperty> imageProperties1 = service.GetImageProperties(null);

            List<ImageProperty> imageProperties2 = service.GetImageProperties(imageProperties1.First().GUID);

            List<CatalogItem> catalogItems = service.GetCatalogItems("false", imageProperties2.First().GUID);
        }
    }
}
