using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using IDBrowserServiceCore.Data;
using IDBrowserServiceCore.Code;
using ImageMagick;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Controllers
{
    [Route("api/[controller]/[action]")]
    [Route("Service.svc/[action]")] //Compatibility to old service
    public class ValuesController : Controller
    {
        private ILogger log;
        private IConfiguration configuration;
        private TransactionOptions readUncommittedTransactionOptions;
        private IDImagerDB db;
        private IDImagerThumbsDB dbThumbs;

        public ValuesController(IDImagerDB db, IDImagerThumbsDB dbThumbs, IConfiguration configuration, 
            ILoggerFactory DepLoggerFactory, IHostingEnvironment DepHostingEnvironment)
        {
            this.db = db;
            this.dbThumbs = dbThumbs;
            this.configuration = configuration;

            if (log == null)
                log = DepLoggerFactory.CreateLogger("Controllers.ValuesController");

            readUncommittedTransactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            };
        }
        new public void Dispose()
        {
            try
            {
                base.Dispose();

                if (db != null)
                    db.Dispose();

                if (dbThumbs != null)
                    dbThumbs.Dispose();
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }

        [ActionName("GetStatus")]
        public string GetStatus()
        {
            return "online";
        }

        [HttpGet("{guid?}")]
        [ActionName("GetImageProperties")]
        public async Task<ActionResult<List<ImageProperty>>> GetImageProperties(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                    LogHttpConnection("GetImageProperties");
                else
                    LogHttpConnection(string.Format("GetImageProperties with guid: {0}", guid));

                List<ImageProperty> listImageProperty;

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

                    listImageProperty = await query.ToListAsync();
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

                    listImageProperty = await query.ToListAsync();
                }

                return listImageProperty;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [HttpGet("{guid}/{isCategory}")]
        [ActionName("GetImagePropertyThumbnail")]
        public async Task<ActionResult<Stream>> GetImagePropertyThumbnail(string guid, string isCategory)
        {
            try
            {
                return await GetImagePropertyThumbnailStream(guid, isCategory);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw;
            }
        }

        [HttpGet("{guid}/{isCategory}/{width}/{height}")]
        [ActionName("GetResizedImagePropertyThumbnail")]
        public async Task<ActionResult<Stream>> GetResizedImagePropertyThumbnail(string guid, string isCategory, string width, string height)
        {
            return await GetImagePropertyThumbnailStream(guid, isCategory, width, height);
        }

        private async Task<ActionResult<Stream>> GetImagePropertyThumbnailStream(string guid, string isCategory, string width = null, string height = null)
        {
            byte[] idImage;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                if (Boolean.Parse(isCategory))
                {
                    idImage = await db.idPropCategory.Where(x => x.GUID == guid).Select(x => x.idImage).SingleAsync();
                }
                else
                {
                    idImage = await db.idProp.Where(x => x.GUID == guid).Select(x => x.idImage).SingleAsync();
                }

                scope.Complete();
            }

            Stream imageStream = null;
            if (idImage == null)
            {
                LogHttpConnection(string.Format("GetImagePropertyThumbnail with guid: {0} isCategory: {1} (returned null)",
                    guid, isCategory));
            }
            else
            {
                if (HttpContext != null)
                    HttpContext.Response.ContentType = "image/jpeg";
                
                imageStream = new MemoryStream(idImage);
                LogHttpConnection(string.Format("GetImagePropertyThumbnail with guid: {0} isCategory: {1}",
                    guid, isCategory));
            }

            return imageStream;
        }

        [HttpGet("{orderDescending}/{propertyGuid}")]
        [ActionName("GetCatalogItems")]
        public async Task<ActionResult<List<CatalogItem>>> GetCatalogItems(string orderDescending, string propertyGuid)
        {
            try
            {
                List<CatalogItem> catalogItems = null;

                var query = from tbl in db.idCatalogItemDefinition
                            where tbl.GUID == propertyGuid
                            select tbl;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                    readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (Boolean.Parse(orderDescending))
                    {
                        query = query.OrderByDescending(x => x.idCatalogItem.DateTimeStamp);
                    }
                    else
                    {
                        query = query.OrderBy(x => x.idCatalogItem.DateTimeStamp);
                    }

                    catalogItems = await query.Select(x => new CatalogItem
                    {
                        GUID = x.CatalogItemGUID,
                        FileName = x.idCatalogItem.FileName,
                        FileType = x.idCatalogItem.idFileType,
                        FilePath = x.idCatalogItem.idFilePath.FilePath,
                        HasRecipe = x.idCatalogItem.idHasRecipe
                    }).ToListAsync();

                    scope.Complete();
                }

                LogHttpConnection(string.Format("GetCatalogItems with orderDescending: {0} propertyGuid: {1}",
                     orderDescending, propertyGuid));

                return catalogItems;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [HttpGet("{orderDescending}/{filePathGuid}")]
        [ActionName("GetCatalogItemsByFilePath")]
        public async Task<ActionResult<List<CatalogItem>>> GetCatalogItemsByFilePath(string orderDescending, string filePathGuid)
        {
            try
            {
                List<CatalogItem> catalogItems = null;

                var query = from tbl in db.idCatalogItem
                            where tbl.PathGUID == filePathGuid
                            select tbl;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                    readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (Boolean.Parse(orderDescending))
                    {
                        query = query.OrderByDescending(x => x.DateTimeStamp);
                    }
                    else
                    {
                        query = query.OrderBy(x => x.DateTimeStamp);
                    }

                    catalogItems = await query.Select(x => new CatalogItem
                    {
                        GUID = x.GUID,
                        FileName = x.FileName,
                        FileType = x.idFileType,
                        FilePath = x.idFilePath.FilePath,
                        HasRecipe = x.idHasRecipe
                    }).ToListAsync();

                    scope.Complete();
                }

                LogHttpConnection(string.Format("GetCatalogItemsByFilePath with orderDescending: {0} filePathGuid: {1}",
                    orderDescending, filePathGuid));
                return catalogItems;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [HttpGet("{type}/{imageGuid}")]
        [ActionName("GetImageThumbnail")]
        public async Task<ActionResult<Stream>> GetImageThumbnail(string type, string imageGuid)
        {
            try
            {
                return await GetImageThumbnailStream(type, imageGuid);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        private async Task<ActionResult<Stream>> GetImageThumbnailStream(string type, string imageGuid, string width = null, string height = null)
        {
            if (type != "T" && type != "R" && type != "M")
                throw new Exception("Unsupported image type");

            idCatalogItem catalogItem = null;
            Stream imageStream = null;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                catalogItem = await db.idCatalogItem.Include(x => x.idFilePath).SingleAsync(x => x.GUID == imageGuid);

                if (catalogItem == null)
                    throw new Exception("CatalogItem not found");

                if (type == "R" && catalogItem.idHasRecipe == 0)
                    throw new Exception("Image has no recipe");

                //Check if CatalogItem has a recipe, if yes try to get the recipe image
                if (type == "M" && catalogItem.idHasRecipe > 0)
                    type = "R";

                Boolean keepAspectRatio = Boolean.Parse(configuration["IDBrowserServiceSettings:KeepAspectRatio"]);
                Boolean setGenericVideoThumbnailOnError = Boolean.Parse(configuration["IDBrowserServiceSettings:SetGenericVideoThumbnailOnError"]);

                idThumbs thumb = null;

                // Distributed transaction are not supported
                using (var scope2 = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                {
                    //Searching with FirstOrDefault because PhotoSupreme sometimes stores the Thumbnail twice
                    thumb = await dbThumbs.idThumbs.FirstOrDefaultAsync(x => x.ImageGUID == imageGuid && x.idType == type);

                    //If recipe image is not found, return the M image,
                    //because the programm cannot yet generate the recipe image
                    if (thumb == null && type == "R")
                    {
                        type = "M";
                        //Searching with FirstOrDefault because PhotoSupreme sometimes stores the Thumbnail twice
                        thumb = await dbThumbs.idThumbs.FirstOrDefaultAsync(x => x.ImageGUID == imageGuid && x.idType == type);
                    }
                }

                // ToDo: Externe Image Library implementieren für das.
                //if (thumb == null && Boolean.Parse(configuration["IDBrowserServiceSettings:CreateThumbnails"))
                //{
                //    SaveImageThumbnailResult result = StaticFunctions.SaveImageThumbnail(catalogItem, ref db, ref dbThumbs, new List<String>() { type }, keepAspectRatio, setGenericVideoThumbnailOnError);

                //    foreach (Exception ex in result.Exceptions)
                //        log.Error(ex.ToString());

                //    if (result.ImageStreams.Count > 0)
                //    {
                //        imageStream = result.ImageStreams.First();

                //        //if (IsRequestRest()) ??
                //        HttpContext.Response.ContentType = "image/jpeg";

                //        log.LogInformation(String.Format("GetImageThumbnail with type: {2} imageGuid: {3} (returned resizedImageStream)",
                //            HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort, type, imageGuid));
                //    }
                //    else
                //    {
                //        log.LogInformation(String.Format("GetImageThumbnail with type: {2} imageGuid: {3} (returned null)",
                //            HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort, type, imageGuid));
                //    }
                //}
                //else
                //{
                imageStream = new MemoryStream(thumb.idThumb);

                if (HttpContext != null)
                    HttpContext.Response.ContentType = "image/jpeg";

                LogHttpConnection(string.Format("GetImageThumbnail with type: {0} imageGuid: {1} (returned imageStream)",
                    type, imageGuid));
                //}

                scope.Complete();
            }

            // ToDo: Externe Image Library implementieren für das.
            //if (width != null && height != null)
            //{
            //    TransformGroup transformGroup = new TransformGroup();
            //    BitmapSource bitmapSource = StaticFunctions.GetBitmapFrameFromImageStream(imageStream, "JPG");
            //    StaticFunctions.Resize(ref bitmapSource, ref transformGroup, int.Parse(width), int.Parse(height));

            //    if (transformGroup != null && transformGroup.Children.Count > 0)
            //    {
            //        TransformedBitmap tb = new TransformedBitmap();
            //        tb.BeginInit();
            //        tb.Source = bitmapSource;
            //        tb.Transform = transformGroup;
            //        tb.EndInit();

            //        bitmapSource = tb;

            //        BitmapFrame transformedBitmapFrame = BitmapFrame.Create(bitmapSource);

            //        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            //        encoder.Frames.Add(transformedBitmapFrame);
            //        imageStream = new System.IO.MemoryStream();
            //        encoder.Save(imageStream);
            //        imageStream.Position = 0;
            //    }
            //}

            return imageStream;
        }

        [HttpGet("{imageGuid}")]
        [ActionName("GetImage")]
        public async Task<ActionResult<Stream>> GetImage(string imageGuid)
        {
            Stream imageStream = null;
            try
            {
                LogHttpConnection(string.Format("GetImage with imageGuid: {0}", imageGuid));

                imageStream = await GetImageStream(imageGuid);

                if (HttpContext != null)
                {
                    HttpContext.Response.ContentType = "image/jpeg";
                    HttpContext.Response.Headers.Add("Content-Size", imageStream.Length.ToString());
                }

                return imageStream;
            }
            catch (Exception ex)
            {
                if (imageStream != null) { imageStream.Close(); }
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [HttpGet("{width}/{height}/{imageGuid}")]
        [ActionName("GetResizedImage")]
        public async Task<ActionResult<Stream>> GetResizedImage(string width, string height, string imageGuid)
        {
            Stream imageStream = null;
            try
            {
                LogHttpConnection(string.Format("GetResizedImage with width: {0} height {1} imageGuid: {2}",
                    width, height, imageGuid));

                imageStream = await GetImageStream(imageGuid, width, height);

                if (HttpContext != null)
                {
                    HttpContext.Response.ContentType = "image/jpeg";
                    HttpContext.Response.Headers.Add("Content-Size", imageStream.Length.ToString());
                }

                return imageStream;
            }
            catch (Exception ex)
            {
                if (imageStream != null) { imageStream.Close(); }
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        private async Task<Stream> GetImageStream(string imageGuid, string width = null, string height = null)
        {
            idCatalogItem catalogItem = null;
            Boolean keepAspectRatio = Boolean.Parse(configuration["IDBrowserServiceSettings:KeepAspectRatio"]);

            catalogItem = await db.idCatalogItem.Include(x => x.idFilePath).SingleAsync(x => x.GUID == imageGuid);
            if (catalogItem == null)
                throw new Exception("CatalogItem not found");

            string strImageFilePath = StaticFunctions.GetImageFilePath(catalogItem);
            Stream imageStream = StaticFunctions.GetImageFileStream(strImageFilePath);

            XmpReceipe xmpReceipe = null;
            try
            {
                System.Xml.Linq.XDocument recipeXDocument = await StaticFunctions.GetRecipeXDocument(db, catalogItem);
                xmpReceipe = XmpRecipeHelper.ParseXmlRecepie(recipeXDocument);
            }
            catch (Exception ex)
            {
                log.LogError(String.Format("Error in 'GetImageStream' when applying recipe on imageGuid {0}: {1}",
                    catalogItem.GUID, ex.ToString()));
            }

            if ((width != null && height != null) || (xmpReceipe != null && xmpReceipe.HasValues ))
            {
                MagickImage image = new MagickImage(imageStream);
                int intWidth = int.Parse(width);
                int intHeight = int.Parse(height);

                if (xmpReceipe == null)
                    XmpRecipeHelper.ApplyXmpRecipe(xmpReceipe, image);

                if (image.Width > intWidth && image.Height > intHeight)
                    image.Resize(intWidth, intHeight);

                //ToDo: Herausfinden ob rotation noch nötig ist.
                //Rotation rotation = StaticFunctions.Rotate(ref bitmapSource, ref transformGroup);

                imageStream = new MemoryStream();
                image.Write(imageStream);

                imageStream.Position = 0;
            }

            return imageStream;
        }

        [HttpGet("{imageGuid}")]
        [ActionName("GetImageInfo")]
        public async Task<ActionResult<ImageInfo>> GetImageInfo(string imageGuid)
        {
            try
            {
                var queryXMP = from tbl in db.idSearchData
                               where tbl.RelatedGUID == imageGuid
                               && tbl.ContentType.Equals("XMP")
                               select new XmpProperty { Name = tbl.ContentGroup, Value = tbl.ContentValue };

                ImageInfo imageInfo = null;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                    readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                {
                    imageInfo = await (from tbl in db.idCatalogItem
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
                                 }).SingleAsync();

                    imageInfo.XmpProperties = await queryXMP.Distinct().ToListAsync();

                    scope.Complete();
                }

                LogHttpConnection(string.Format("GetImageInfo with imageGuid: {0}", imageGuid));
                return imageInfo;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [ActionName("GetRandomImageGuids")]
        public async Task<ActionResult<List<String>>> GetRandomImageGuids()
        {
            try
            {
                List<String> randomImageGuids;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                    readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                {
                    var queryRandom = from x in db.idCatalogItem
                                      where StaticFunctions.ImageFileExtensions.Contains(x.idFileType)
                                      orderby Guid.NewGuid()
                                      select x.GUID;
                    randomImageGuids = await queryRandom.Take(100).ToListAsync();
                    scope.Complete();
                }

                LogHttpConnection("GetRandomImageGuids");
                return randomImageGuids;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [HttpGet("{imageGuid}")]
        [ActionName("GetFile")]
        public async Task<ActionResult<Stream>> GetFile(string imageGuid)
        {
            Stream fileStream = null;
            try
            {
                LogHttpConnection(string.Format("GetFile with imageGuid: {0}", imageGuid));

                idCatalogItem catalogItem;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                    readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                {
                    catalogItem = await db.idCatalogItem.Include(x => x.idFilePath).SingleAsync(x => x.GUID == imageGuid);
                    scope.Complete();
                }

                if (catalogItem == null)
                    throw new Exception("CatalogItem not found");

                string strImageFilePath = StaticFunctions.GetImageFilePath(catalogItem);
                fileStream = StaticFunctions.GetImageFileStream(strImageFilePath);

                if (HttpContext != null)
                {
                    HttpContext.Response.ContentType = "application/octet-stream";
                    HttpContext.Response.Headers.Add("Content-Size", fileStream.Length.ToString());
                }

                return fileStream;
            }
            catch (Exception ex)
            {
                if (fileStream != null) { fileStream.Close(); }
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        //private bool IsRequestRest()
        //{
        //    return OperationContext.Current.EndpointDispatcher.ChannelDispatcher.BindingName.Equals("http://tempuri.org/:WebHttpBinding");
        //}

        [HttpGet("{searchString}")]
        [ActionName("SearchImageProperties")]
        public async Task<ActionResult<List<ImagePropertyRecursive>>> SearchImageProperties(string searchString)
        {
            try
            {
                var queryProperties = from tbl in db.idProp
                                      where tbl.PropName.Contains(searchString)
                                      orderby tbl.PropName
                                      select tbl;

                List<ImagePropertyRecursive> listImagePropertyRecursive;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                    readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                {
                    listImagePropertyRecursive = await GetImagePropertyRecursive(queryProperties);
                    scope.Complete();
                }

                LogHttpConnection(string.Format("SearchImagePropertiesSoap with searchString: {0}", searchString));
                return listImagePropertyRecursive;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [HttpGet("{catalogItemGUID}")]
        [ActionName("GetCatalogItemImageProperties")]
        public async Task<ActionResult<List<ImagePropertyRecursive>>> GetCatalogItemImageProperties(string catalogItemGUID)
        {
            try
            {
                var queryCatalogItemDefinition = from tbl in db.idCatalogItemDefinition
                                                 where tbl.CatalogItemGUID == catalogItemGUID
                                                 select tbl.GUID;

                List<String> propertyGuids;
                List<ImagePropertyRecursive> listImagePropertyRecursive;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                    readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                {
                    propertyGuids = await queryCatalogItemDefinition.ToListAsync();

                    var queryProperties = from tbl in db.idProp
                                          where propertyGuids.Contains(tbl.GUID)
                                          orderby tbl.PropName
                                          select tbl;

                    listImagePropertyRecursive = await GetImagePropertyRecursive(queryProperties);
                    scope.Complete();
                }

                LogHttpConnection(string.Format("GetCatalogItemImageProperties with catalogItemGUID: {0}",
                    catalogItemGUID));
                return listImagePropertyRecursive;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        private async Task<List<ImagePropertyRecursive>> GetImagePropertyRecursive(IQueryable<idProp> query)
        {
            List<ImagePropertyRecursive> listImagePropertyRecursive = new List<ImagePropertyRecursive>();
            foreach (idProp row in query)
            {
                String parentGuid = row.ParentGUID;
                List<String> parentProperties = new List<string>();

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                    readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                {
                    while (parentGuid != null)
                    {
                        idProp parentProperty = await db.idProp.SingleOrDefaultAsync(x => x.GUID == parentGuid);

                        if (parentProperty == null)
                        {
                            idPropCategory parentPropCategory = await db.idPropCategory.SingleOrDefaultAsync(x => x.GUID == parentGuid);
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

                ImagePropertyRecursive imagePropertyRecursive = new ImagePropertyRecursive
                {
                    GUID = row.GUID,
                    Name = row.PropName,
                    FullRecursivePath = String.Join("::", parentProperties)
                };

                listImagePropertyRecursive.Add(imagePropertyRecursive);
            }

            return listImagePropertyRecursive;
        }

        [HttpGet("{propertyGuid}/{catalogItemGUID}")]
        [ActionName("AddCatalogItemDefinition")]
        public async Task<ActionResult<string>> AddCatalogItemDefinition(string propertyGuid, string catalogItemGUID)
        {
            try
            {
                if (await db.idProp.Where(x => x.GUID == propertyGuid).CountAsync() == 0)
                    throw new Exception("Image property does not exist");

                var query = from tbl in db.idCatalogItemDefinition
                            where tbl.GUID == propertyGuid & tbl.CatalogItemGUID == catalogItemGUID
                            select tbl;

                if (await query.CountAsync() > 0)
                    throw new Exception("CatalogItemDefinition already exists");

                idCatalogItem currentIdCatalogItem = await db.idCatalogItem.SingleAsync(x => x.GUID == catalogItemGUID);
                idImageVersion currentIdImageVersion = await db.idImageVersion.SingleOrDefaultAsync(x => x.MainImageGUID == catalogItemGUID);

                idCatalogItemDefinition newIdCatalogItemDefinition = new idCatalogItemDefinition
                {
                    GUID = propertyGuid,
                    CatalogItemGUID = catalogItemGUID,
                    idAssigned = DateTime.Now
                };

                db.idCatalogItemDefinition.Add(newIdCatalogItemDefinition);

                currentIdCatalogItem.idInSync = currentIdCatalogItem.idInSync >> 2;
                if (currentIdImageVersion != null)
                    currentIdImageVersion.idInSync = currentIdImageVersion.idInSync >> 2;

                await db.SaveChangesAsync();

                LogHttpConnection(string.Format("AddCatalogItemDefinition with propertyGuid: {0} and catalogItemGUID {1}",
                    propertyGuid, catalogItemGUID));
                return "OK";
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [HttpGet("{propertyGuid}/{catalogItemGUID}")]
        [ActionName("DeleteCatalogItemDefinition")]
        public async Task<ActionResult<string>> DeleteCatalogItemDefinition(string propertyGuid, string catalogItemGUID)
        {
            try
            {
                idCatalogItemDefinition currentIdCatalogItemDefinition = await db.idCatalogItemDefinition
                    .SingleAsync(x => x.GUID == propertyGuid && x.CatalogItemGUID == catalogItemGUID);
                idCatalogItem currentIdCatalogItem = await db.idCatalogItem
                    .SingleAsync(x => x.GUID == catalogItemGUID);
                idImageVersion currentIdImageVersion = await db.idImageVersion
                    .SingleOrDefaultAsync(x => x.MainImageGUID == catalogItemGUID);

                db.idCatalogItemDefinition.Remove(currentIdCatalogItemDefinition);

                currentIdCatalogItem.idInSync = currentIdCatalogItem.idInSync >> 2;
                if (currentIdImageVersion != null)
                    currentIdImageVersion.idInSync = currentIdImageVersion.idInSync >> 2;

                await db.SaveChangesAsync();

                LogHttpConnection(string.Format("DeleteCatalogItemDefinition with propertyGuid: {0} and catalogItemGUID {1}",
                    propertyGuid, catalogItemGUID));
                return "OK";
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [HttpGet("{propertyName}/{parentGUID}")]
        [ActionName("AddImageProperty")]
        public async Task<ActionResult<string>> AddImageProperty(string propertyName, string parentGUID)
        {
            try
            {
                idProp parentIdProp = await db.idProp.SingleOrDefaultAsync(x => x.GUID.Equals(parentGUID));
                int? parentRgt;

                if (parentIdProp == null)
                    parentRgt = await db.idProp.Where(x => x.ParentGUID.Equals(parentGUID)).MaxAsync(x => x.rgt);
                else
                    parentRgt = parentIdProp.rgt;

                idUser user = await db.idUser.FirstAsync();
                DateTime dtNow = DateTime.Now;
                String guid = Guid.NewGuid().ToString();

                while (await db.idProp.Where(x => x.GUID == guid).CountAsync() > 0)
                {
                    guid = Guid.NewGuid().ToString();
                }

                //Update nested set rgt
                var queryUpdateNestedSetRgt = from tbl in db.idProp
                                              where tbl.rgt >= parentRgt
                                              select tbl;

                foreach (idProp row in await queryUpdateNestedSetRgt.ToListAsync())
                {
                    row.rgt = row.rgt + 2;
                }

                //Update nested set lft
                var queryUpdateNestedSetLft = from tbl in db.idProp
                                              where tbl.lft > parentRgt
                                              select tbl;

                foreach (idProp row in await queryUpdateNestedSetLft.ToListAsync())
                {
                    row.lft = row.lft + 2;
                }

                idProp newIdProp = new idProp
                {
                    GUID = guid,
                    ParentGUID = parentGUID,
                    PropName = propertyName,
                    PropValue = "",
                    Quick = 0,
                    UserGUID = user.GUID,
                    idCreated = dtNow,
                    idLastAccess = dtNow,
                    PropXMPLink = "",
                    lft = parentRgt,
                    rgt = parentRgt + 1,
                    idImage = null,
                    ParentAssign = 0,
                    ParentXMPLinkAssign = 0,
                    idSynonyms = "",
                    idGPSLon = 90,
                    idGPSLat = 90,
                    idGPSAlt = 0,
                    idGPSGeoTag = 0,
                    idGPSGeoTagIfExist = 0,
                    idGPSRadius = 0,
                    idShortCut = 0,
                    MutualExclusive = 0,
                    idDescription = "",
                    idDetails = null,
                    idProps = null,
                    ApplyProps = 0
                };

                db.idProp.Add(newIdProp);
                await db.SaveChangesAsync();

                LogHttpConnection(string.Format("AddImageProperty with propertyName: {0}, parentGUID {1}, lft {2}, rgt {3}",
                    propertyName, parentGUID, newIdProp.lft, newIdProp.rgt));
                return "OK";
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [ActionName("GetFilePaths")]
        public async Task<ActionResult<List<FilePath>>> GetFilePaths()
        {
            try
            {
                List<FilePath> listFilePaths;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                    readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
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

                    listFilePaths = await query.ToListAsync();
                }

                LogHttpConnection("GetFilePaths");
                return listFilePaths;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        private void LogHttpConnection(string callingMethod)
        {
            if (HttpContext != null && HttpContext.Connection != null)
            {
                log.LogInformation(String.Format("Client {0}:{1} called {2}", 
                    HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort, callingMethod));
            }
            else
            {
                log.LogInformation(String.Format("Called {0}", callingMethod));
            }
        }
    }
}
