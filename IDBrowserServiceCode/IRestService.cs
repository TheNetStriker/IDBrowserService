using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;

namespace IDBrowserServiceCode
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IRestService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/GetImageProperties/{guid=null}", ResponseFormat = WebMessageFormat.Xml)]
        List<ImageProperty> GetImageProperties(string guid);

        [OperationContract]
        [WebGet(UriTemplate = "/GetImagePropertyThumbnail/{guid}/{isCategory}", ResponseFormat = WebMessageFormat.Xml)]
        Stream GetImagePropertyThumbnail(string guid, string isCategory);

        [OperationContract]
        [WebGet(UriTemplate = "/GetCatalogItems/{orderDescending}/{propertyGuid}", ResponseFormat = WebMessageFormat.Xml)]
        List<CatalogItem> GetCatalogItems(string orderDescending, string propertyGuid);

        [OperationContract]
        [WebGet(UriTemplate = "/GetCatalogItemsByFilePath/{orderDescending}/{filePathGuid}", ResponseFormat = WebMessageFormat.Xml)]
        List<CatalogItem> GetCatalogItemsByFilePath(string orderDescending, string filePathGuid);

        [OperationContract]
        [WebGet(UriTemplate = "/GetImageThumbnail/{type}/{imageGuid}", ResponseFormat = WebMessageFormat.Xml)]
        Stream GetImageThumbnail(string type, string imageGuid);

        [OperationContract]
        [WebGet(UriTemplate = "/GetImage/{imageGuid}", ResponseFormat = WebMessageFormat.Xml)]
        Stream GetImage(string imageGuid);

        [OperationContract]
        [WebGet(UriTemplate = "/GetResizedImage/{width}/{height}/{imageGuid}", ResponseFormat = WebMessageFormat.Xml)]
        Stream GetResizedImage(string width, string height, string imageGuid);

        [OperationContract]
        [WebGet(UriTemplate = "/GetImageInfo/{imageGuid}", ResponseFormat = WebMessageFormat.Xml)]
        ImageInfo GetImageInfo(string imageGuid);

        [OperationContract]
        [WebGet(UriTemplate = "/GetRandomImageGuids/", ResponseFormat = WebMessageFormat.Xml)]
        List<String> GetRandomImageGuids();

        [OperationContract]
        [WebGet(UriTemplate = "/SearchImageProperties/{searchString}", ResponseFormat = WebMessageFormat.Xml)]
        List<ImagePropertyRecursive> SearchImageProperties(string searchString);

        [OperationContract]
        [WebGet(UriTemplate = "/GetCatalogItemImageProperties/{catalogItemGUID}", ResponseFormat = WebMessageFormat.Xml)]
        List<ImagePropertyRecursive> GetCatalogItemImageProperties(string catalogItemGUID);

        [OperationContract]
        [WebGet(UriTemplate = "/AddCatalogItemDefinition/{propertyGuid}/{catalogItemGUID}", ResponseFormat = WebMessageFormat.Xml)]
        string AddCatalogItemDefinition(string propertyGuid, string catalogItemGUID);

        [OperationContract]
        [WebGet(UriTemplate = "/DeleteCatalogItemDefinition/{propertyGuid}/{catalogItemGUID}", ResponseFormat = WebMessageFormat.Xml)]
        string DeleteCatalogItemDefinition(string propertyGuid, string catalogItemGUID);

        [OperationContract]
        [WebGet(UriTemplate = "/AddImageProperty/{propertyName}/{parentGUID}", ResponseFormat = WebMessageFormat.Xml)]
        string AddImageProperty(string propertyName, string parentGUID);

        [OperationContract]
        [WebGet(UriTemplate = "/GetFile/{imageGuid}", ResponseFormat = WebMessageFormat.Xml)]
        Stream GetFile(string imageGuid);

        [OperationContract]
        [WebGet(UriTemplate = "/GetFilePaths/", ResponseFormat = WebMessageFormat.Xml)]
        List<FilePath> GetFilePaths();

        //[OperationContract]
        //[WebGet(UriTemplate = "/GetVideoStream/{imageGuid}", ResponseFormat = WebMessageFormat.Xml)]
        //Stream GetVideoStream(string imageGuid);
    }
}
