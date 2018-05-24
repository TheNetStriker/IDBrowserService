using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using System.Web;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using IDBrowserServiceCode;
using System.Configuration;
using Common.Logging;
using System.ServiceModel.Channels;
using ComponentAce.Compression.Libs.zlib;
using System.Net.Mime;
using IDBrowserServiceCode.Data;
using System.Transactions;
using System.Data.Entity.Core.EntityClient;

namespace IDBrowserServiceCode
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Single)]
    public class Service: IRestService, ISoapService, IDisposable
    {
        private Data.IDImagerDB db;
        private Data.IDImagerDB dbThumbs;
        private RemoteEndpointMessageProperty clientEndpoint;
        private ILog log;
        private TransactionOptions readUncommittedTransactionOptions;

        public Service()
        {
            if (log == null)
                log = log = LogManager.GetCurrentClassLogger();

            try
            {
                readUncommittedTransactionOptions = new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                };

                if (OperationContext.Current != null)
                    clientEndpoint = OperationContext.Current.IncomingMessageProperties["System.ServiceModel.Channels.RemoteEndpointMessageProperty"] as RemoteEndpointMessageProperty;

                if (db == null)
                    db = new IDImagerDB();

                if (db.Database.Connection.State == System.Data.ConnectionState.Closed)
                    db.Database.Connection.Open();

                if (dbThumbs == null)
                {
                    dbThumbs = new IDImagerDB();
                    //EntityConnectionStringBuilder ecb = new EntityConnectionStringBuilder(ConfigurationManager.ConnectionStrings["IDImagerThumbsEntities"].ConnectionString);
                    dbThumbs.Database.Connection.ConnectionString = ConfigurationManager.ConnectionStrings["IDImagerThumbsEntities"].ConnectionString;
                }

                if (dbThumbs.Database.Connection.State == System.Data.ConnectionState.Closed)
                    dbThumbs.Database.Connection.Open();
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        public void Dispose()
        {
            try
            {
                if (db != null)
                {
                    db.Database.Connection.Close();
                    db.Dispose();
                }

                if (dbThumbs != null)
                {
                    dbThumbs.Database.Connection.Close();
                    dbThumbs.Dispose();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        public List<ImageProperty> GetImageProperties(string guid)
        {
            try
            {
                List<ImageProperty> listImageProperty;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, readUncommittedTransactionOptions))
                {
                    if (guid == null)
                    {
                        var query = from tbl in db.v_PropCategory
                                    where !tbl.CategoryName.Equals("Internal")
                                    orderby tbl.CategoryName
                                    select new ImageProperty
                                    {
                                        GUID = tbl.GUID,
                                        Name = tbl.CategoryName,
                                        ImageCount = tbl.idPhotoCount,
                                        SubPropertyCount = db.idProp.Where(x => x.ParentGUID == tbl.GUID).Count()
                                    };

                        listImageProperty = query.ToList();
                    }
                    else
                    {
                        var query = from tbl in db.v_prop
                                    where tbl.ParentGUID == guid
                                    orderby tbl.PropName
                                    select new ImageProperty
                                    {
                                        GUID = tbl.GUID,
                                        Name = tbl.PropName,
                                        ImageCount = tbl.idPhotoCount,
                                        SubPropertyCount = db.idProp.Where(x => x.ParentGUID == tbl.GUID).Count()
                                    };

                        listImageProperty = query.ToList();
                    }

                    if (clientEndpoint != null)
                        log.Info(String.Format("Client {0}:{1} called GetImageProperties with guid: {2}",
                            clientEndpoint.Address, clientEndpoint.Port, guid));

                    scope.Complete();
                }
                
                return listImageProperty;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }

        public Stream GetImagePropertyThumbnail(string guid, string isCategory)
        {
            try
            {
                return GetImagePropertyThumbnailStream(guid, isCategory);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw;
            }
        }

        public Stream GetResizedImagePropertyThumbnail(string guid, string isCategory, string width, string height)
        {
            return GetImagePropertyThumbnailStream(guid, isCategory, width, height);
        }

        private Stream GetImagePropertyThumbnailStream(string guid, string isCategory, string width = null, string height = null)
        {
            byte[] idImage;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, readUncommittedTransactionOptions))
            {
                if (Boolean.Parse(isCategory))
                {
                    idImage = db.idPropCategory.Single(x => x.GUID == guid).idImage;
                }
                else
                {
                    idImage = db.idProp.Single(x => x.GUID == guid).idImage;
                }

                scope.Complete();
            }
            
            Stream imageStream = null;
            if (idImage == null)
            {
                log.Info(String.Format("Client {0}:{1} called GetImagePropertyThumbnail with guid: {2} isCategory: {3} (returned null)",
                    clientEndpoint.Address, clientEndpoint.Port, guid, isCategory));
            }
            else
            {
                if (IsRequestRest())
                    WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";

                imageStream = new MemoryStream(idImage);
                log.Info(String.Format("Client {0}:{1} called GetImagePropertyThumbnail with guid: {2} isCategory: {3}",
                    clientEndpoint.Address, clientEndpoint.Port, guid.ToString(), isCategory));
            }

            return imageStream;
        }

        public List<CatalogItem> GetCatalogItems(string orderDescending, string propertyGuid)
        {
            try
            {
                List<CatalogItem> catalogItems = null;

                var query = from tbl in db.idCatalogItemDefinition
                            where tbl.GUID == propertyGuid
                            select tbl;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, readUncommittedTransactionOptions))
                {
                    if (Boolean.Parse(orderDescending))
                    {
                        query = query.OrderByDescending(x => x.idCatalogItem.DateTimeStamp);
                    }
                    else
                    {
                        query = query.OrderBy(x => x.idCatalogItem.DateTimeStamp);
                    }

                    catalogItems = query.Select(x => new CatalogItem
                    {
                        GUID = x.CatalogItemGUID,
                        FileName = x.idCatalogItem.FileName,
                        FileType = x.idCatalogItem.idFileType,
                        FilePath = x.idCatalogItem.idFilePath.FilePath,
                        HasRecipe = x.idCatalogItem.idHasRecipe
                    }).ToList();

                    scope.Complete();
                }

                if (clientEndpoint != null)
                    log.Info(String.Format("Client {0}:{1} called GetCatalogItems with orderDescending: {2} propertyGuid: {3}",
                        clientEndpoint.Address, clientEndpoint.Port, orderDescending, propertyGuid));

                return catalogItems;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }

        public List<CatalogItem> GetCatalogItemsByFilePath(string orderDescending, string filePathGuid)
        {
            try
            {
                List<CatalogItem> catalogItems = null;

                var query = from tbl in db.idCatalogItem
                            where tbl.PathGUID == filePathGuid
                            select tbl;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, readUncommittedTransactionOptions))
                {
                    if (Boolean.Parse(orderDescending))
                    {
                        query = query.OrderByDescending(x => x.DateTimeStamp);
                    }
                    else
                    {
                        query = query.OrderBy(x => x.DateTimeStamp);
                    }

                    catalogItems = query.Select(x => new CatalogItem
                    {
                        GUID = x.GUID,
                        FileName = x.FileName,
                        FileType = x.idFileType,
                        FilePath = x.idFilePath.FilePath,
                        HasRecipe = x.idHasRecipe
                    }).ToList();

                    scope.Complete();
                }

                log.Info(String.Format("Client {0}:{1} called GetCatalogItemsByFilePath with orderDescending: {2} filePathGuid: {3}",
                    clientEndpoint.Address, clientEndpoint.Port, orderDescending, filePathGuid));
                return catalogItems;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }

        public Stream GetImageThumbnail(string type, string imageGuid)
        {
            try
            {
                return GetImageThumbnailStream(type, imageGuid);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }

        private Stream GetImageThumbnailStream(string type, string imageGuid, string width = null, string height = null)
        {
            if (type != "T" && type != "R" && type != "M")
                throw new Exception("Unsupported image type");

            idCatalogItem catalogItem = null;
            Stream imageStream = null;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, readUncommittedTransactionOptions))
            {
                catalogItem = db.idCatalogItem.Include("idFilePath").Single(x => x.GUID == imageGuid);

                if (catalogItem == null)
                    throw new Exception("CatalogItem not found");

                if (type == "R" && catalogItem.idHasRecipe == 0)
                    throw new Exception("Image has no recipe");

                //Check if CatalogItem has a recipe, if yes try to get the recipe image
                if (type == "M" && catalogItem.idHasRecipe > 0)
                    type = "R";

                Boolean keepAspectRatio = Boolean.Parse(ConfigurationManager.AppSettings["KeepAspectRatio"]);
                Boolean setGenericVideoThumbnailOnError = Boolean.Parse(ConfigurationManager.AppSettings["SetGenericVideoThumbnailOnError"]);

                idThumbs thumb = null;
                lock (dbThumbs)
                {
                    thumb = dbThumbs.idThumbs.SingleOrDefault(x => x.ImageGUID == imageGuid && x.idType == type);

                    //If recipe image is not found, return the M image,
                    //because the programm cannot yet generate the recipe image
                    if (thumb == null && type == "R")
                    {
                        type = "M";
                        thumb = dbThumbs.idThumbs.SingleOrDefault(x => x.ImageGUID == imageGuid && x.idType == type);
                    }
                }

                if (thumb == null && Boolean.Parse(ConfigurationManager.AppSettings["CreateThumbnails"]))
                {
                    SaveImageThumbnailResult result = StaticFunctions.SaveImageThumbnail(catalogItem, ref db, ref dbThumbs, new List<String>() { type }, keepAspectRatio, setGenericVideoThumbnailOnError);

                    foreach (Exception ex in result.Exceptions)
                        log.Error(ex.ToString());

                    if (result.ImageStreams.Count > 0)
                    {
                        imageStream = result.ImageStreams.First();

                        if (IsRequestRest())
                            WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";

                        log.Info(String.Format("Client {0}:{1} called GetImageThumbnail with type: {2} imageGuid: {3} (returned resizedImageStream)",
                            clientEndpoint.Address, clientEndpoint.Port, type, imageGuid));
                    }
                    else
                    {
                        log.Info(String.Format("Client {0}:{1} called GetImageThumbnail with type: {2} imageGuid: {3} (returned null)",
                            clientEndpoint.Address, clientEndpoint.Port, type, imageGuid));
                    }
                }
                else
                {
                    imageStream = new MemoryStream(thumb.idThumb);

                    if (IsRequestRest())
                        WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";

                    log.Info(String.Format("Client {0}:{1} called GetImageThumbnail with type: {2} imageGuid: {3} (returned imageStream)",
                        clientEndpoint.Address, clientEndpoint.Port, type, imageGuid));
                }

                scope.Complete();
            }
                        
            if (width != null && height != null)
            {
                TransformGroup transformGroup = new TransformGroup();
                BitmapSource bitmapSource = StaticFunctions.GetBitmapFrameFromImageStream(imageStream, "JPG");
                StaticFunctions.Resize(ref bitmapSource, ref transformGroup, int.Parse(width), int.Parse(height));

                if (transformGroup != null && transformGroup.Children.Count > 0)
                {
                    TransformedBitmap tb = new TransformedBitmap();
                    tb.BeginInit();
                    tb.Source = bitmapSource;
                    tb.Transform = transformGroup;
                    tb.EndInit();

                    bitmapSource = tb;

                    BitmapFrame transformedBitmapFrame = BitmapFrame.Create(bitmapSource);

                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(transformedBitmapFrame);
                    imageStream = new System.IO.MemoryStream();
                    encoder.Save(imageStream);
                    imageStream.Position = 0;
                }
            }

            return imageStream;
        }

        public Stream GetImage(string imageGuid)
        {
            Stream imageStream = null;
            try
            {
                log.Info(String.Format("Client {0}:{1} called GetImage with imageGuid: {2}",
                    clientEndpoint.Address, clientEndpoint.Port, imageGuid));

                imageStream = GetImageStream(imageGuid);

                if (IsRequestRest())
                {
                    WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";
                    WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Size", imageStream.Length.ToString());
                }
                
                return imageStream;
            }
            catch (Exception ex)
            {
                if (imageStream != null) { imageStream.Close(); }
                log.Error(ex.ToString());
                throw ex;
            }
        }

        public Stream GetResizedImage(string width, string height, string imageGuid)
        {
            Stream imageStream = null;
            try
            {
                log.Info(String.Format("Client {0}:{1} called GetResizedImage with width: {2} height {3} imageGuid: {4}",
                    clientEndpoint.Address, clientEndpoint.Port, width, height, imageGuid));

                imageStream = GetImageStream(imageGuid, width, height);

                if (IsRequestRest())
                {
                    WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";
                    WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Size", imageStream.Length.ToString());
                }

                return imageStream;
            }
            catch (Exception ex)
            {
                if (imageStream != null) { imageStream.Close(); }
                log.Error(ex.ToString());
                throw ex;
            }
        }

        private Stream GetImageStream(string imageGuid, string width = null, string height = null)
        {
            idCatalogItem catalogItem = null;
            Boolean keepAspectRatio = Boolean.Parse(ConfigurationManager.AppSettings["KeepAspectRatio"]);

            catalogItem = db.idCatalogItem.Include("idFilePath").Single(x => x.GUID == imageGuid);
            if (catalogItem == null)
                throw new Exception("CatalogItem not found");

            Stream imageStream = StaticFunctions.GetImageFileStream(StaticFunctions.GetImageFilePath(catalogItem));
            BitmapSource bitmapSource = StaticFunctions.GetBitmapFrameFromImageStream(imageStream, catalogItem.idFileType);

            System.Xml.Linq.XDocument recipeXDocument = null;
            try
            {
                recipeXDocument = StaticFunctions.GetRecipeXDocument(db, catalogItem);
            }
            catch (Exception ex)
            {
                log.Error(String.Format("Error in 'GetImageStream' when applying recipe on imageGuid {0}: {1}",
                    catalogItem.GUID, ex.ToString()));
            }

            TransformGroup transformGroup = new TransformGroup();

            if (width != null && height != null)
                StaticFunctions.Resize(ref bitmapSource, ref transformGroup, int.Parse(width), int.Parse(height));

            Rotation rotation = StaticFunctions.Rotate(ref bitmapSource, ref transformGroup);

            if (Recipe.ApplyXmpRecipe(recipeXDocument, ref bitmapSource, transformGroup))
            {
                BitmapFrame transformedBitmapFrame = BitmapFrame.Create(bitmapSource);

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(transformedBitmapFrame);
                imageStream = new System.IO.MemoryStream();
                encoder.Save(imageStream);
            }

            imageStream.Position = 0;
            return imageStream;
        }

        public ImageInfo GetImageInfo(string imageGuid)
        {
            try
            {
                var queryXMP = from tbl in db.idSearchData
                               where tbl.RelatedGUID == imageGuid
                               && tbl.ContentType.Equals("XMP")
                               select new XmpProperty { Name = tbl.ContentGroup, Value = tbl.ContentValue };

                ImageInfo imageInfo = null;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, readUncommittedTransactionOptions))
                {
                    imageInfo = (from tbl in db.idCatalogItem
                                where tbl.GUID == imageGuid
                                select new ImageInfo
                                {
                                    FileSize = tbl.FileSize,
                                    FileType = tbl.idFileType,
                                    ImageDescription = tbl.ImageDescription,
                                    ImageName = tbl.ImageName,
                                    ImageResolution = tbl.UDF2,
                                    Rating = tbl.Rating,
                                    Timestamp = tbl.DateTimeStamp,
                                    GPSLat = tbl.idGPSLat,
                                    GPSLon = tbl.idGPSLon
                                }).Single();

                    imageInfo.XmpProperties = queryXMP.Distinct().ToList();

                    scope.Complete();
                }
                
                log.Info(String.Format("Client {0}:{1} called GetImageInfo with imageGuid: {2}", 
                    clientEndpoint.Address, clientEndpoint.Port, imageGuid));
                return imageInfo;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }

        public List<String> GetRandomImageGuids()
        {
            try
            {
                List<String> randomImageGuids;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, readUncommittedTransactionOptions))
                {
                    var queryRandom = from x in db.idCatalogItem
                                      where StaticFunctions.ImageFileExtensions.Contains(x.idFileType)
                                      orderby Guid.NewGuid()
                                      select x.GUID;
                    randomImageGuids = queryRandom.Take(100).ToList();
                    scope.Complete();
                }
                
                log.Info(String.Format("Client {0}:{1} called GetRandomImageGuids",
                    clientEndpoint.Address, clientEndpoint.Port));
                return randomImageGuids;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }

        public Stream GetFile(string imageGuid)
        {
            Stream fileStream = null;
            try
            {
                log.Info(String.Format("Client {0}:{1} called GetFile with imageGuid: {2}",
                    clientEndpoint.Address, clientEndpoint.Port, imageGuid));

                idCatalogItem catalogItem;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, readUncommittedTransactionOptions))
                {
                    catalogItem = db.idCatalogItem.Include("idFilePath").Single(x => x.GUID == imageGuid);
                    scope.Complete();
                }
                
                if (catalogItem == null)
                    throw new Exception("CatalogItem not found");

                fileStream = StaticFunctions.GetImageFileStream(StaticFunctions.GetImageFilePath(catalogItem));;

                if (IsRequestRest())
                {
                    WebOperationContext.Current.OutgoingResponse.ContentType = "application/octet-stream";
                    WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Size", fileStream.Length.ToString());
                }
                
                return fileStream;
            }
            catch (Exception ex)
            {
                if (fileStream != null) { fileStream.Close(); }
                log.Error(ex.ToString());
                throw ex;
            }
        }

        private bool IsRequestRest()
        {
            return OperationContext.Current.EndpointDispatcher.ChannelDispatcher.BindingName.Equals("http://tempuri.org/:WebHttpBinding");
        }

        //public Stream GetVideoStream(string imageGuid)
        //{
        //    try
        //    {
        //        Init();
        //        lock (db)
        //        {
        //            //WebOperationContext.Current.OutgoingResponse.Headers.Clear();
        //            WebOperationContext.Current.OutgoingResponse.ContentType = "video/mp4";
        //            idCatalogItem catalogItem = db.idCatalogItem.Include("idFilePath").Single(x => x.GUID == imageGuid);
        //            //WebOperationContext.Current.OutgoingResponse.ContentType = "application/octet-stream";
        //            //WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Disposition", String.Format("attachment; filename={0}", catalogItem.FileName));
        //            return StaticFunctions.GetImageFileStream(StaticFunctions.GetImageFilePath(catalogItem));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ErrorOccured != null) { ErrorOccured.Invoke(this, new EventArgs(), ex); };
        //        System.Diagnostics.EventLog.WriteEntry(IDBrowserServiceCode.Properties.Resources.ApplicationSource, ex.ToString());
        //        throw;
        //    }
        //}


        public List<ImagePropertyRecursive> SearchImageProperties(string searchString)
        {
            try
            {
                var queryProperties = from tbl in db.idProp
                                      where tbl.PropName.Contains(searchString)
                                      orderby tbl.PropName
                                      select tbl;

                List<ImagePropertyRecursive> listImagePropertyRecursive;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, readUncommittedTransactionOptions))
                {
                    listImagePropertyRecursive = GetImagePropertyRecursive(queryProperties);
                    scope.Complete();
                }        

                log.Info(String.Format("Client {0}:{1} called SearchImagePropertiesSoap with searchString: {2}",
                    clientEndpoint.Address, clientEndpoint.Port, searchString));
                return listImagePropertyRecursive;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }

        public List<ImagePropertyRecursive> GetCatalogItemImageProperties(string catalogItemGUID)
        {
            try
            {
                var queryCatalogItemDefinition = from tbl in db.idCatalogItemDefinition
                                                 where tbl.CatalogItemGUID == catalogItemGUID
                                                 select tbl.GUID;

                List<String> propertyGuids;
                List<ImagePropertyRecursive> listImagePropertyRecursive;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, readUncommittedTransactionOptions))
                {
                    propertyGuids = queryCatalogItemDefinition.ToList();

                    var queryProperties = from tbl in db.idProp
                                          where propertyGuids.Contains(tbl.GUID)
                                          orderby tbl.PropName
                                          select tbl;

                    listImagePropertyRecursive = GetImagePropertyRecursive(queryProperties);
                    scope.Complete();
                }

                log.Info(String.Format("Client {0}:{1} called GetCatalogItemImageProperties with catalogItemGUID: {2}",
                    clientEndpoint.Address, clientEndpoint.Port, catalogItemGUID));
                return listImagePropertyRecursive;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }

        private List<ImagePropertyRecursive> GetImagePropertyRecursive(IQueryable<idProp> query)
        {
            List<ImagePropertyRecursive> listImagePropertyRecursive = new List<ImagePropertyRecursive>();
            foreach (idProp row in query)
            {
                String parentGuid = row.ParentGUID;
                List<String> parentProperties = new List<string>();

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, readUncommittedTransactionOptions))
                {
                    while (parentGuid != null)
                    {
                        idProp parentProperty = db.idProp.SingleOrDefault(x => x.GUID == parentGuid);

                        if (parentProperty == null)
                        {
                            idPropCategory parentPropCategory = db.idPropCategory.SingleOrDefault(x => x.GUID == parentGuid);
                            if (parentPropCategory != null)
                                parentProperties.Insert(0, parentPropCategory.CategoryName);
                            parentGuid = null;
                        }
                        else
                        {
                            parentProperties.Insert(0, parentProperty.PropName);
                            parentGuid = parentProperty.ParentGUID;
                        }
                    }

                    scope.Complete();
                }

                ImagePropertyRecursive imagePropertyRecursive = new ImagePropertyRecursive();
                imagePropertyRecursive.GUID = row.GUID;
                imagePropertyRecursive.Name = row.PropName;
                imagePropertyRecursive.FullRecursivePath = String.Join("::", parentProperties);
                listImagePropertyRecursive.Add(imagePropertyRecursive);
            }

            return listImagePropertyRecursive;
        }

        public string AddCatalogItemDefinition(string propertyGuid, string catalogItemGUID)
        {
            try
            {
                var query = from tbl in db.idCatalogItemDefinition
                            where tbl.GUID == propertyGuid & tbl.CatalogItemGUID == catalogItemGUID
                            select tbl;

                if (query.Count() > 0)
                    throw new Exception("CatalogItemDefinition already exists");

                idCatalogItem currentIdCatalogItem = db.idCatalogItem.Single(x => x.GUID == catalogItemGUID);
                idImageVersion currentIdImageVersion = db.idImageVersion.SingleOrDefault(x => x.MainImageGUID == catalogItemGUID);

                idCatalogItemDefinition newIdCatalogItemDefinition = new idCatalogItemDefinition();
                newIdCatalogItemDefinition.GUID = propertyGuid;
                newIdCatalogItemDefinition.CatalogItemGUID = catalogItemGUID;
                newIdCatalogItemDefinition.idAssigned = DateTime.Now;
                db.idCatalogItemDefinition.Add(newIdCatalogItemDefinition);

                currentIdCatalogItem.idInSync = currentIdCatalogItem.idInSync >> 2;
                if (currentIdImageVersion != null)
                    currentIdImageVersion.idInSync = currentIdImageVersion.idInSync >> 2;

                db.SaveChanges();

                log.Info(String.Format("Client {0}:{1} called AddCatalogItemDefinition with propertyGuid: {2} and catalogItemGUID {3}",
                    clientEndpoint.Address, clientEndpoint.Port, propertyGuid, catalogItemGUID));
                return "OK";
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }

        public string DeleteCatalogItemDefinition(string propertyGuid, string catalogItemGUID)
        {
            try
            {
                idCatalogItemDefinition currentIdCatalogItemDefinition = db.idCatalogItemDefinition.Single(x => x.GUID == propertyGuid && x.CatalogItemGUID == catalogItemGUID);
                idCatalogItem currentIdCatalogItem = db.idCatalogItem.Single(x => x.GUID == catalogItemGUID);
                idImageVersion currentIdImageVersion = db.idImageVersion.SingleOrDefault(x => x.MainImageGUID == catalogItemGUID);

                db.idCatalogItemDefinition.Remove(currentIdCatalogItemDefinition);

                currentIdCatalogItem.idInSync = currentIdCatalogItem.idInSync >> 2;
                if (currentIdImageVersion != null)
                    currentIdImageVersion.idInSync = currentIdImageVersion.idInSync >> 2;

                db.SaveChanges();

                log.Info(String.Format("Client {0}:{1} called DeleteCatalogItemDefinition with propertyGuid: {2} and catalogItemGUID {3}",
                    clientEndpoint.Address, clientEndpoint.Port, propertyGuid, catalogItemGUID));
                return "OK";
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }

        public string AddImageProperty(string propertyName, string parentGUID)
        {
            try
            {
                idProp parentIdProp = db.idProp.SingleOrDefault(x => x.GUID.Equals(parentGUID));
                int? parentRgt;

                if (parentIdProp == null)
                    parentRgt = db.idProp.Where(x => x.ParentGUID.Equals(parentGUID)).Max(x => x.rgt);
                else
                    parentRgt = parentIdProp.rgt;

                idUser user = db.idUser.First();
                DateTime dtNow = DateTime.Now;
                String guid = Guid.NewGuid().ToString();

                while (db.idProp.Where(x => x.GUID == guid).Count() > 0) {
                    guid = Guid.NewGuid().ToString();
                }

                //Update nested set rgt
                var queryUpdateNestedSetRgt = from tbl in db.idProp
                                              where tbl.rgt >= parentRgt
                                              select tbl;

                foreach (idProp row in queryUpdateNestedSetRgt) {
                    row.rgt = row.rgt + 2;
                }

                //Update nested set lft
                var queryUpdateNestedSetLft = from tbl in db.idProp
                                              where tbl.lft > parentRgt
                                              select tbl;

                foreach (idProp row in queryUpdateNestedSetLft) {
                    row.lft = row.lft + 2;
                }

                idProp newIdProp = new idProp();
                newIdProp.GUID = guid;
                newIdProp.ParentGUID = parentGUID;
                newIdProp.PropName = propertyName;
                newIdProp.PropValue = "";
                newIdProp.Quick = 0;
                newIdProp.UserGUID = user.GUID;
                newIdProp.idCreated = dtNow;
                newIdProp.idLastAccess = dtNow;
                newIdProp.PropXMPLink = "";
                newIdProp.lft = parentRgt;
                newIdProp.rgt = parentRgt + 1;
                newIdProp.idImage = null;
                newIdProp.ParentAssign = 0;
                newIdProp.ParentXMPLinkAssign = 0;
                newIdProp.idSynonyms = "";
                newIdProp.idGPSLon = 90;
                newIdProp.idGPSLat = 90;
                newIdProp.idGPSAlt = 0;
                newIdProp.idGPSGeoTag = 0;
                newIdProp.idGPSGeoTagIfExist = 0;
                newIdProp.idGPSRadius = 0;
                newIdProp.idShortCut = 0;
                newIdProp.MutualExclusive = 0;
                newIdProp.idDescription = "";
                newIdProp.idDetails = null;
                newIdProp.idProps = null;
                newIdProp.ApplyProps = 0;

                db.idProp.Add(newIdProp);
                db.SaveChanges();

                log.Info(String.Format("Client {0}:{1} called AddImageProperty with propertyName: {2}, parentGUID {3}, lft {4}, rgt {5}",
                    clientEndpoint.Address, clientEndpoint.Port, propertyName, parentGUID, newIdProp.lft, newIdProp.rgt));
                return "OK";
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }

        public List<FilePath> GetFilePaths()
        {
            try
            {
                List<FilePath> listFilePaths;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, readUncommittedTransactionOptions))
                {
                    var query = from tbl in db.idFilePath
                                orderby tbl.FilePath
                                select new FilePath
                                {
                                    GUID = tbl.guid,
                                    MediumName = tbl.MediumName,
                                    Path = tbl.FilePath,
                                    RootName = tbl.RootName,
                                    ImageCount = tbl.idCatalogItem.Count()
                                };

                    listFilePaths = query.ToList();
                }
                
                log.Info(String.Format("Client {0}:{1} called GetFilePaths",
                    clientEndpoint.Address, clientEndpoint.Port));
                return listFilePaths;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }
    }
}
