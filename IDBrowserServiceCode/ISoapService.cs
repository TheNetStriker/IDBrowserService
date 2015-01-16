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
    public interface ISoapService
    {
        [OperationContract(Name = "GetImagePropertiesSoap")]
        List<ImageProperty> GetImageProperties(string guid);

        [OperationContract(Name = "GetImagePropertyThumbnailSoap")]
        Stream GetImagePropertyThumbnail(string guid, string isCategory);

        [OperationContract(Name = "GetCatalogItemsSoap")]
        List<CatalogItem> GetCatalogItems(string orderDescending, string propertyGuid);

        [OperationContract(Name = "GetCatalogItemsByFilePathSoap")]
        List<CatalogItem> GetCatalogItemsByFilePath(string orderDescending, string filePathGuid);

        [OperationContract(Name = "GetImageThumbnailSoap")]
        Stream GetImageThumbnail(string type, string imageGuid);

        [OperationContract(Name = "GetImageSoap")]
        Stream GetImage(string imageGuid);

        [OperationContract(Name = "GetResizedImageSoap")]
        Stream GetResizedImage(string width, string height, string imageGuid);

        [OperationContract(Name = "GetImageInfoSoap")]
        ImageInfo GetImageInfo(string GetImageInfo);

        [OperationContract(Name = "GetRandomImageGuidsSoap")]
        List<String> GetRandomImageGuids();

        [OperationContract(Name = "SearchImagePropertiesSoap")]
        List<ImagePropertyRecursive> SearchImageProperties(string searchString);

        [OperationContract(Name = "GetCatalogItemImagePropertiesSoap")]
        List<ImagePropertyRecursive> GetCatalogItemImageProperties(string catalogItemGUID);

        [OperationContract(Name = "AddCatalogItemDefinitionSoap")]
        string AddCatalogItemDefinition(string propertyGuid, string catalogItemGUID);

        [OperationContract(Name = "DeleteCatalogItemDefinitionSoap")]
        string DeleteCatalogItemDefinition(string propertyGuid, string catalogItemGUID);

        [OperationContract(Name = "AddImagePropertySoap")]
        string AddImageProperty(string propertyName, string parentGUID);

        [OperationContract(Name = "GetFileSoap")]
        Stream GetFile(string imageGuid);

        [OperationContract(Name = "GetFilePathsSoap")]
        List<FilePath> GetFilePaths();

        //[OperationContract]
        //[WebGet(UriTemplate = "/GetVideoStream/{imageGuid}", ResponseFormat = WebMessageFormat.Xml)]
        //Stream GetVideoStream(string imageGuid);
    }
}
