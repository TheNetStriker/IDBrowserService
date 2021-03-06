﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IDBrowserServiceTest.IDBrowserService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="IDBrowserService.IRestService")]
    public interface IRestService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetImageProperties", ReplyAction="http://tempuri.org/IRestService/GetImagePropertiesResponse")]
        System.Collections.Generic.List<IDBrowserServiceCode.ImageProperty> GetImageProperties(string guid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetImageProperties", ReplyAction="http://tempuri.org/IRestService/GetImagePropertiesResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.ImageProperty>> GetImagePropertiesAsync(string guid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetImagePropertyThumbnail", ReplyAction="http://tempuri.org/IRestService/GetImagePropertyThumbnailResponse")]
        System.IO.Stream GetImagePropertyThumbnail(string guid, string isCategory);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetImagePropertyThumbnail", ReplyAction="http://tempuri.org/IRestService/GetImagePropertyThumbnailResponse")]
        System.Threading.Tasks.Task<System.IO.Stream> GetImagePropertyThumbnailAsync(string guid, string isCategory);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetCatalogItems", ReplyAction="http://tempuri.org/IRestService/GetCatalogItemsResponse")]
        System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem> GetCatalogItems(string orderDescending, string propertyGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetCatalogItems", ReplyAction="http://tempuri.org/IRestService/GetCatalogItemsResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem>> GetCatalogItemsAsync(string orderDescending, string propertyGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetCatalogItemsByFilePath", ReplyAction="http://tempuri.org/IRestService/GetCatalogItemsByFilePathResponse")]
        System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem> GetCatalogItemsByFilePath(string orderDescending, string filePathGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetCatalogItemsByFilePath", ReplyAction="http://tempuri.org/IRestService/GetCatalogItemsByFilePathResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem>> GetCatalogItemsByFilePathAsync(string orderDescending, string filePathGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetImageThumbnail", ReplyAction="http://tempuri.org/IRestService/GetImageThumbnailResponse")]
        System.IO.Stream GetImageThumbnail(string type, string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetImageThumbnail", ReplyAction="http://tempuri.org/IRestService/GetImageThumbnailResponse")]
        System.Threading.Tasks.Task<System.IO.Stream> GetImageThumbnailAsync(string type, string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetImage", ReplyAction="http://tempuri.org/IRestService/GetImageResponse")]
        System.IO.Stream GetImage(string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetImage", ReplyAction="http://tempuri.org/IRestService/GetImageResponse")]
        System.Threading.Tasks.Task<System.IO.Stream> GetImageAsync(string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetResizedImage", ReplyAction="http://tempuri.org/IRestService/GetResizedImageResponse")]
        System.IO.Stream GetResizedImage(string width, string height, string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetResizedImage", ReplyAction="http://tempuri.org/IRestService/GetResizedImageResponse")]
        System.Threading.Tasks.Task<System.IO.Stream> GetResizedImageAsync(string width, string height, string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetImageInfo", ReplyAction="http://tempuri.org/IRestService/GetImageInfoResponse")]
        IDBrowserServiceCode.ImageInfo GetImageInfo(string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetImageInfo", ReplyAction="http://tempuri.org/IRestService/GetImageInfoResponse")]
        System.Threading.Tasks.Task<IDBrowserServiceCode.ImageInfo> GetImageInfoAsync(string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetRandomImageGuids", ReplyAction="http://tempuri.org/IRestService/GetRandomImageGuidsResponse")]
        System.Collections.Generic.List<string> GetRandomImageGuids();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetRandomImageGuids", ReplyAction="http://tempuri.org/IRestService/GetRandomImageGuidsResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<string>> GetRandomImageGuidsAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/SearchImageProperties", ReplyAction="http://tempuri.org/IRestService/SearchImagePropertiesResponse")]
        System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive> SearchImageProperties(string searchString);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/SearchImageProperties", ReplyAction="http://tempuri.org/IRestService/SearchImagePropertiesResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive>> SearchImagePropertiesAsync(string searchString);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetCatalogItemImageProperties", ReplyAction="http://tempuri.org/IRestService/GetCatalogItemImagePropertiesResponse")]
        System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive> GetCatalogItemImageProperties(string catalogItemGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetCatalogItemImageProperties", ReplyAction="http://tempuri.org/IRestService/GetCatalogItemImagePropertiesResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive>> GetCatalogItemImagePropertiesAsync(string catalogItemGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/AddCatalogItemDefinition", ReplyAction="http://tempuri.org/IRestService/AddCatalogItemDefinitionResponse")]
        string AddCatalogItemDefinition(string propertyGuid, string catalogItemGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/AddCatalogItemDefinition", ReplyAction="http://tempuri.org/IRestService/AddCatalogItemDefinitionResponse")]
        System.Threading.Tasks.Task<string> AddCatalogItemDefinitionAsync(string propertyGuid, string catalogItemGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/DeleteCatalogItemDefinition", ReplyAction="http://tempuri.org/IRestService/DeleteCatalogItemDefinitionResponse")]
        string DeleteCatalogItemDefinition(string propertyGuid, string catalogItemGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/DeleteCatalogItemDefinition", ReplyAction="http://tempuri.org/IRestService/DeleteCatalogItemDefinitionResponse")]
        System.Threading.Tasks.Task<string> DeleteCatalogItemDefinitionAsync(string propertyGuid, string catalogItemGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/AddImageProperty", ReplyAction="http://tempuri.org/IRestService/AddImagePropertyResponse")]
        string AddImageProperty(string propertyName, string parentGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/AddImageProperty", ReplyAction="http://tempuri.org/IRestService/AddImagePropertyResponse")]
        System.Threading.Tasks.Task<string> AddImagePropertyAsync(string propertyName, string parentGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetFile", ReplyAction="http://tempuri.org/IRestService/GetFileResponse")]
        System.IO.Stream GetFile(string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetFile", ReplyAction="http://tempuri.org/IRestService/GetFileResponse")]
        System.Threading.Tasks.Task<System.IO.Stream> GetFileAsync(string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetFilePaths", ReplyAction="http://tempuri.org/IRestService/GetFilePathsResponse")]
        System.Collections.Generic.List<IDBrowserServiceCode.FilePath> GetFilePaths();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRestService/GetFilePaths", ReplyAction="http://tempuri.org/IRestService/GetFilePathsResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.FilePath>> GetFilePathsAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IRestServiceChannel : IDBrowserServiceTest.IDBrowserService.IRestService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class RestServiceClient : System.ServiceModel.ClientBase<IDBrowserServiceTest.IDBrowserService.IRestService>, IDBrowserServiceTest.IDBrowserService.IRestService {
        
        public RestServiceClient() {
        }
        
        public RestServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public RestServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public RestServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public RestServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Collections.Generic.List<IDBrowserServiceCode.ImageProperty> GetImageProperties(string guid) {
            return base.Channel.GetImageProperties(guid);
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.ImageProperty>> GetImagePropertiesAsync(string guid) {
            return base.Channel.GetImagePropertiesAsync(guid);
        }
        
        public System.IO.Stream GetImagePropertyThumbnail(string guid, string isCategory) {
            return base.Channel.GetImagePropertyThumbnail(guid, isCategory);
        }
        
        public System.Threading.Tasks.Task<System.IO.Stream> GetImagePropertyThumbnailAsync(string guid, string isCategory) {
            return base.Channel.GetImagePropertyThumbnailAsync(guid, isCategory);
        }
        
        public System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem> GetCatalogItems(string orderDescending, string propertyGuid) {
            return base.Channel.GetCatalogItems(orderDescending, propertyGuid);
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem>> GetCatalogItemsAsync(string orderDescending, string propertyGuid) {
            return base.Channel.GetCatalogItemsAsync(orderDescending, propertyGuid);
        }
        
        public System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem> GetCatalogItemsByFilePath(string orderDescending, string filePathGuid) {
            return base.Channel.GetCatalogItemsByFilePath(orderDescending, filePathGuid);
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem>> GetCatalogItemsByFilePathAsync(string orderDescending, string filePathGuid) {
            return base.Channel.GetCatalogItemsByFilePathAsync(orderDescending, filePathGuid);
        }
        
        public System.IO.Stream GetImageThumbnail(string type, string imageGuid) {
            return base.Channel.GetImageThumbnail(type, imageGuid);
        }
        
        public System.Threading.Tasks.Task<System.IO.Stream> GetImageThumbnailAsync(string type, string imageGuid) {
            return base.Channel.GetImageThumbnailAsync(type, imageGuid);
        }
        
        public System.IO.Stream GetImage(string imageGuid) {
            return base.Channel.GetImage(imageGuid);
        }
        
        public System.Threading.Tasks.Task<System.IO.Stream> GetImageAsync(string imageGuid) {
            return base.Channel.GetImageAsync(imageGuid);
        }
        
        public System.IO.Stream GetResizedImage(string width, string height, string imageGuid) {
            return base.Channel.GetResizedImage(width, height, imageGuid);
        }
        
        public System.Threading.Tasks.Task<System.IO.Stream> GetResizedImageAsync(string width, string height, string imageGuid) {
            return base.Channel.GetResizedImageAsync(width, height, imageGuid);
        }
        
        public IDBrowserServiceCode.ImageInfo GetImageInfo(string imageGuid) {
            return base.Channel.GetImageInfo(imageGuid);
        }
        
        public System.Threading.Tasks.Task<IDBrowserServiceCode.ImageInfo> GetImageInfoAsync(string imageGuid) {
            return base.Channel.GetImageInfoAsync(imageGuid);
        }
        
        public System.Collections.Generic.List<string> GetRandomImageGuids() {
            return base.Channel.GetRandomImageGuids();
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<string>> GetRandomImageGuidsAsync() {
            return base.Channel.GetRandomImageGuidsAsync();
        }
        
        public System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive> SearchImageProperties(string searchString) {
            return base.Channel.SearchImageProperties(searchString);
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive>> SearchImagePropertiesAsync(string searchString) {
            return base.Channel.SearchImagePropertiesAsync(searchString);
        }
        
        public System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive> GetCatalogItemImageProperties(string catalogItemGUID) {
            return base.Channel.GetCatalogItemImageProperties(catalogItemGUID);
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive>> GetCatalogItemImagePropertiesAsync(string catalogItemGUID) {
            return base.Channel.GetCatalogItemImagePropertiesAsync(catalogItemGUID);
        }
        
        public string AddCatalogItemDefinition(string propertyGuid, string catalogItemGUID) {
            return base.Channel.AddCatalogItemDefinition(propertyGuid, catalogItemGUID);
        }
        
        public System.Threading.Tasks.Task<string> AddCatalogItemDefinitionAsync(string propertyGuid, string catalogItemGUID) {
            return base.Channel.AddCatalogItemDefinitionAsync(propertyGuid, catalogItemGUID);
        }
        
        public string DeleteCatalogItemDefinition(string propertyGuid, string catalogItemGUID) {
            return base.Channel.DeleteCatalogItemDefinition(propertyGuid, catalogItemGUID);
        }
        
        public System.Threading.Tasks.Task<string> DeleteCatalogItemDefinitionAsync(string propertyGuid, string catalogItemGUID) {
            return base.Channel.DeleteCatalogItemDefinitionAsync(propertyGuid, catalogItemGUID);
        }
        
        public string AddImageProperty(string propertyName, string parentGUID) {
            return base.Channel.AddImageProperty(propertyName, parentGUID);
        }
        
        public System.Threading.Tasks.Task<string> AddImagePropertyAsync(string propertyName, string parentGUID) {
            return base.Channel.AddImagePropertyAsync(propertyName, parentGUID);
        }
        
        public System.IO.Stream GetFile(string imageGuid) {
            return base.Channel.GetFile(imageGuid);
        }
        
        public System.Threading.Tasks.Task<System.IO.Stream> GetFileAsync(string imageGuid) {
            return base.Channel.GetFileAsync(imageGuid);
        }
        
        public System.Collections.Generic.List<IDBrowserServiceCode.FilePath> GetFilePaths() {
            return base.Channel.GetFilePaths();
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.FilePath>> GetFilePathsAsync() {
            return base.Channel.GetFilePathsAsync();
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="IDBrowserService.ISoapService")]
    public interface ISoapService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetImagePropertiesSoap", ReplyAction="http://tempuri.org/ISoapService/GetImagePropertiesSoapResponse")]
        System.Collections.Generic.List<IDBrowserServiceCode.ImageProperty> GetImagePropertiesSoap(string guid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetImagePropertiesSoap", ReplyAction="http://tempuri.org/ISoapService/GetImagePropertiesSoapResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.ImageProperty>> GetImagePropertiesSoapAsync(string guid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetImagePropertyThumbnailSoap", ReplyAction="http://tempuri.org/ISoapService/GetImagePropertyThumbnailSoapResponse")]
        System.IO.Stream GetImagePropertyThumbnailSoap(string guid, string isCategory);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetImagePropertyThumbnailSoap", ReplyAction="http://tempuri.org/ISoapService/GetImagePropertyThumbnailSoapResponse")]
        System.Threading.Tasks.Task<System.IO.Stream> GetImagePropertyThumbnailSoapAsync(string guid, string isCategory);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetCatalogItemsSoap", ReplyAction="http://tempuri.org/ISoapService/GetCatalogItemsSoapResponse")]
        System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem> GetCatalogItemsSoap(string orderDescending, string propertyGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetCatalogItemsSoap", ReplyAction="http://tempuri.org/ISoapService/GetCatalogItemsSoapResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem>> GetCatalogItemsSoapAsync(string orderDescending, string propertyGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetCatalogItemsByFilePathSoap", ReplyAction="http://tempuri.org/ISoapService/GetCatalogItemsByFilePathSoapResponse")]
        System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem> GetCatalogItemsByFilePathSoap(string orderDescending, string filePathGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetCatalogItemsByFilePathSoap", ReplyAction="http://tempuri.org/ISoapService/GetCatalogItemsByFilePathSoapResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem>> GetCatalogItemsByFilePathSoapAsync(string orderDescending, string filePathGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetImageThumbnailSoap", ReplyAction="http://tempuri.org/ISoapService/GetImageThumbnailSoapResponse")]
        System.IO.Stream GetImageThumbnailSoap(string type, string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetImageThumbnailSoap", ReplyAction="http://tempuri.org/ISoapService/GetImageThumbnailSoapResponse")]
        System.Threading.Tasks.Task<System.IO.Stream> GetImageThumbnailSoapAsync(string type, string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetImageSoap", ReplyAction="http://tempuri.org/ISoapService/GetImageSoapResponse")]
        System.IO.Stream GetImageSoap(string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetImageSoap", ReplyAction="http://tempuri.org/ISoapService/GetImageSoapResponse")]
        System.Threading.Tasks.Task<System.IO.Stream> GetImageSoapAsync(string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetResizedImageSoap", ReplyAction="http://tempuri.org/ISoapService/GetResizedImageSoapResponse")]
        System.IO.Stream GetResizedImageSoap(string width, string height, string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetResizedImageSoap", ReplyAction="http://tempuri.org/ISoapService/GetResizedImageSoapResponse")]
        System.Threading.Tasks.Task<System.IO.Stream> GetResizedImageSoapAsync(string width, string height, string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetImageInfoSoap", ReplyAction="http://tempuri.org/ISoapService/GetImageInfoSoapResponse")]
        IDBrowserServiceCode.ImageInfo GetImageInfoSoap(string GetImageInfo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetImageInfoSoap", ReplyAction="http://tempuri.org/ISoapService/GetImageInfoSoapResponse")]
        System.Threading.Tasks.Task<IDBrowserServiceCode.ImageInfo> GetImageInfoSoapAsync(string GetImageInfo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetRandomImageGuidsSoap", ReplyAction="http://tempuri.org/ISoapService/GetRandomImageGuidsSoapResponse")]
        System.Collections.Generic.List<string> GetRandomImageGuidsSoap();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetRandomImageGuidsSoap", ReplyAction="http://tempuri.org/ISoapService/GetRandomImageGuidsSoapResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<string>> GetRandomImageGuidsSoapAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/SearchImagePropertiesSoap", ReplyAction="http://tempuri.org/ISoapService/SearchImagePropertiesSoapResponse")]
        System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive> SearchImagePropertiesSoap(string searchString);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/SearchImagePropertiesSoap", ReplyAction="http://tempuri.org/ISoapService/SearchImagePropertiesSoapResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive>> SearchImagePropertiesSoapAsync(string searchString);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetCatalogItemImagePropertiesSoap", ReplyAction="http://tempuri.org/ISoapService/GetCatalogItemImagePropertiesSoapResponse")]
        System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive> GetCatalogItemImagePropertiesSoap(string catalogItemGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetCatalogItemImagePropertiesSoap", ReplyAction="http://tempuri.org/ISoapService/GetCatalogItemImagePropertiesSoapResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive>> GetCatalogItemImagePropertiesSoapAsync(string catalogItemGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/AddCatalogItemDefinitionSoap", ReplyAction="http://tempuri.org/ISoapService/AddCatalogItemDefinitionSoapResponse")]
        string AddCatalogItemDefinitionSoap(string propertyGuid, string catalogItemGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/AddCatalogItemDefinitionSoap", ReplyAction="http://tempuri.org/ISoapService/AddCatalogItemDefinitionSoapResponse")]
        System.Threading.Tasks.Task<string> AddCatalogItemDefinitionSoapAsync(string propertyGuid, string catalogItemGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/DeleteCatalogItemDefinitionSoap", ReplyAction="http://tempuri.org/ISoapService/DeleteCatalogItemDefinitionSoapResponse")]
        string DeleteCatalogItemDefinitionSoap(string propertyGuid, string catalogItemGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/DeleteCatalogItemDefinitionSoap", ReplyAction="http://tempuri.org/ISoapService/DeleteCatalogItemDefinitionSoapResponse")]
        System.Threading.Tasks.Task<string> DeleteCatalogItemDefinitionSoapAsync(string propertyGuid, string catalogItemGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/AddImagePropertySoap", ReplyAction="http://tempuri.org/ISoapService/AddImagePropertySoapResponse")]
        string AddImagePropertySoap(string propertyName, string parentGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/AddImagePropertySoap", ReplyAction="http://tempuri.org/ISoapService/AddImagePropertySoapResponse")]
        System.Threading.Tasks.Task<string> AddImagePropertySoapAsync(string propertyName, string parentGUID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetFileSoap", ReplyAction="http://tempuri.org/ISoapService/GetFileSoapResponse")]
        System.IO.Stream GetFileSoap(string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetFileSoap", ReplyAction="http://tempuri.org/ISoapService/GetFileSoapResponse")]
        System.Threading.Tasks.Task<System.IO.Stream> GetFileSoapAsync(string imageGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetFilePathsSoap", ReplyAction="http://tempuri.org/ISoapService/GetFilePathsSoapResponse")]
        System.Collections.Generic.List<IDBrowserServiceCode.FilePath> GetFilePathsSoap();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISoapService/GetFilePathsSoap", ReplyAction="http://tempuri.org/ISoapService/GetFilePathsSoapResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.FilePath>> GetFilePathsSoapAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ISoapServiceChannel : IDBrowserServiceTest.IDBrowserService.ISoapService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class SoapServiceClient : System.ServiceModel.ClientBase<IDBrowserServiceTest.IDBrowserService.ISoapService>, IDBrowserServiceTest.IDBrowserService.ISoapService {
        
        public SoapServiceClient() {
        }
        
        public SoapServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public SoapServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SoapServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SoapServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Collections.Generic.List<IDBrowserServiceCode.ImageProperty> GetImagePropertiesSoap(string guid) {
            return base.Channel.GetImagePropertiesSoap(guid);
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.ImageProperty>> GetImagePropertiesSoapAsync(string guid) {
            return base.Channel.GetImagePropertiesSoapAsync(guid);
        }
        
        public System.IO.Stream GetImagePropertyThumbnailSoap(string guid, string isCategory) {
            return base.Channel.GetImagePropertyThumbnailSoap(guid, isCategory);
        }
        
        public System.Threading.Tasks.Task<System.IO.Stream> GetImagePropertyThumbnailSoapAsync(string guid, string isCategory) {
            return base.Channel.GetImagePropertyThumbnailSoapAsync(guid, isCategory);
        }
        
        public System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem> GetCatalogItemsSoap(string orderDescending, string propertyGuid) {
            return base.Channel.GetCatalogItemsSoap(orderDescending, propertyGuid);
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem>> GetCatalogItemsSoapAsync(string orderDescending, string propertyGuid) {
            return base.Channel.GetCatalogItemsSoapAsync(orderDescending, propertyGuid);
        }
        
        public System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem> GetCatalogItemsByFilePathSoap(string orderDescending, string filePathGuid) {
            return base.Channel.GetCatalogItemsByFilePathSoap(orderDescending, filePathGuid);
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.CatalogItem>> GetCatalogItemsByFilePathSoapAsync(string orderDescending, string filePathGuid) {
            return base.Channel.GetCatalogItemsByFilePathSoapAsync(orderDescending, filePathGuid);
        }
        
        public System.IO.Stream GetImageThumbnailSoap(string type, string imageGuid) {
            return base.Channel.GetImageThumbnailSoap(type, imageGuid);
        }
        
        public System.Threading.Tasks.Task<System.IO.Stream> GetImageThumbnailSoapAsync(string type, string imageGuid) {
            return base.Channel.GetImageThumbnailSoapAsync(type, imageGuid);
        }
        
        public System.IO.Stream GetImageSoap(string imageGuid) {
            return base.Channel.GetImageSoap(imageGuid);
        }
        
        public System.Threading.Tasks.Task<System.IO.Stream> GetImageSoapAsync(string imageGuid) {
            return base.Channel.GetImageSoapAsync(imageGuid);
        }
        
        public System.IO.Stream GetResizedImageSoap(string width, string height, string imageGuid) {
            return base.Channel.GetResizedImageSoap(width, height, imageGuid);
        }
        
        public System.Threading.Tasks.Task<System.IO.Stream> GetResizedImageSoapAsync(string width, string height, string imageGuid) {
            return base.Channel.GetResizedImageSoapAsync(width, height, imageGuid);
        }
        
        public IDBrowserServiceCode.ImageInfo GetImageInfoSoap(string GetImageInfo) {
            return base.Channel.GetImageInfoSoap(GetImageInfo);
        }
        
        public System.Threading.Tasks.Task<IDBrowserServiceCode.ImageInfo> GetImageInfoSoapAsync(string GetImageInfo) {
            return base.Channel.GetImageInfoSoapAsync(GetImageInfo);
        }
        
        public System.Collections.Generic.List<string> GetRandomImageGuidsSoap() {
            return base.Channel.GetRandomImageGuidsSoap();
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<string>> GetRandomImageGuidsSoapAsync() {
            return base.Channel.GetRandomImageGuidsSoapAsync();
        }
        
        public System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive> SearchImagePropertiesSoap(string searchString) {
            return base.Channel.SearchImagePropertiesSoap(searchString);
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive>> SearchImagePropertiesSoapAsync(string searchString) {
            return base.Channel.SearchImagePropertiesSoapAsync(searchString);
        }
        
        public System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive> GetCatalogItemImagePropertiesSoap(string catalogItemGUID) {
            return base.Channel.GetCatalogItemImagePropertiesSoap(catalogItemGUID);
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.ImagePropertyRecursive>> GetCatalogItemImagePropertiesSoapAsync(string catalogItemGUID) {
            return base.Channel.GetCatalogItemImagePropertiesSoapAsync(catalogItemGUID);
        }
        
        public string AddCatalogItemDefinitionSoap(string propertyGuid, string catalogItemGUID) {
            return base.Channel.AddCatalogItemDefinitionSoap(propertyGuid, catalogItemGUID);
        }
        
        public System.Threading.Tasks.Task<string> AddCatalogItemDefinitionSoapAsync(string propertyGuid, string catalogItemGUID) {
            return base.Channel.AddCatalogItemDefinitionSoapAsync(propertyGuid, catalogItemGUID);
        }
        
        public string DeleteCatalogItemDefinitionSoap(string propertyGuid, string catalogItemGUID) {
            return base.Channel.DeleteCatalogItemDefinitionSoap(propertyGuid, catalogItemGUID);
        }
        
        public System.Threading.Tasks.Task<string> DeleteCatalogItemDefinitionSoapAsync(string propertyGuid, string catalogItemGUID) {
            return base.Channel.DeleteCatalogItemDefinitionSoapAsync(propertyGuid, catalogItemGUID);
        }
        
        public string AddImagePropertySoap(string propertyName, string parentGUID) {
            return base.Channel.AddImagePropertySoap(propertyName, parentGUID);
        }
        
        public System.Threading.Tasks.Task<string> AddImagePropertySoapAsync(string propertyName, string parentGUID) {
            return base.Channel.AddImagePropertySoapAsync(propertyName, parentGUID);
        }
        
        public System.IO.Stream GetFileSoap(string imageGuid) {
            return base.Channel.GetFileSoap(imageGuid);
        }
        
        public System.Threading.Tasks.Task<System.IO.Stream> GetFileSoapAsync(string imageGuid) {
            return base.Channel.GetFileSoapAsync(imageGuid);
        }
        
        public System.Collections.Generic.List<IDBrowserServiceCode.FilePath> GetFilePathsSoap() {
            return base.Channel.GetFilePathsSoap();
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<IDBrowserServiceCode.FilePath>> GetFilePathsSoapAsync() {
            return base.Channel.GetFilePathsSoapAsync();
        }
    }
}
