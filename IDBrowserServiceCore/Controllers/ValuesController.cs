using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Transactions;
using IDBrowserServiceCore.Data;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IDBrowserServiceCore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ValuesController : Controller
    {
        private ILogger log;
        private IConfiguration configuration;
        private TransactionOptions readUncommittedTransactionOptions;
        private IDImagerDB db;
        private IDImagerDB dbThumbs;

        public ValuesController(IConfiguration configuration, ILoggerFactory DepLoggerFactory, IHostingEnvironment DepHostingEnvironment)
        {
            this.configuration = configuration;

            if (log == null)
                log = DepLoggerFactory.CreateLogger("Controllers.ValuesController");

            readUncommittedTransactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            };

            if (db == null)
                db = new IDImagerDB(configuration["ConnectionStrings:IDImager"]);

            if (dbThumbs == null)
                dbThumbs = new IDImagerDB(configuration["ConnectionStrings:IDImagerThumbs"]);
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

        [HttpGet("{guid?}")]
        [ActionName("GetImageProperties")]
        public List<ImageProperty> GetImageProperties(string guid)
        {
            try
            {
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

                return listImageProperty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("{guid}/{isCategory}")]
        [ActionName("GetImagePropertyThumbnail")]
        public Stream GetImagePropertyThumbnail(string guid, string isCategory)
        {
            try
            {
                return GetImagePropertyThumbnailStream(guid, isCategory);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw;
            }
        }

        [HttpGet("{guid}/{isCategory}/{width}/{height}")]
        [ActionName("GetResizedImagePropertyThumbnail")]
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
                log.LogInformation(String.Format("Client {0}:{1} called GetImagePropertyThumbnail with guid: {2} isCategory: {3} (returned null)",
                    HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort, guid, isCategory));
            }
            else
            {
                //if (IsRequestRest()) ??
                HttpContext.Response.ContentType = "image/jpeg";
                
                imageStream = new MemoryStream(idImage);
                log.LogInformation(String.Format("Client {0}:{1} called GetImagePropertyThumbnail with guid: {2} isCategory: {3}",
                    HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort, guid.ToString(), isCategory));
            }

            return imageStream;
        }

        [HttpGet("{orderDescending}/{propertyGuid}")]
        [ActionName("GetCatalogItems")]
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

                if (HttpContext.Connection != null)
                    log.LogInformation(String.Format("Client {0}:{1} called GetCatalogItems with orderDescending: {2} propertyGuid: {3}",
                        HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort, orderDescending, propertyGuid));

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

                log.LogInformation(String.Format("Client {0}:{1} called GetCatalogItemsByFilePath with orderDescending: {2} filePathGuid: {3}",
                    HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort, orderDescending, filePathGuid));
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
        public Stream GetImageThumbnail(string type, string imageGuid)
        {
            try
            {
                return GetImageThumbnailStream(type, imageGuid);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
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

                Boolean keepAspectRatio = Boolean.Parse(configuration["IDBrowserServiceSettings:KeepAspectRatio"]);
                Boolean setGenericVideoThumbnailOnError = Boolean.Parse(configuration["IDBrowserServiceSettings:SetGenericVideoThumbnailOnError"]);

                idThumbs thumb = null;
                lock (dbThumbs)
                {
                    //Searching with FirstOrDefault because PhotoSupreme sometimes stores the Thumbnail twice
                    thumb = dbThumbs.idThumbs.FirstOrDefault(x => x.ImageGUID == imageGuid && x.idType == type);

                    //If recipe image is not found, return the M image,
                    //because the programm cannot yet generate the recipe image
                    if (thumb == null && type == "R")
                    {
                        type = "M";
                        //Searching with FirstOrDefault because PhotoSupreme sometimes stores the Thumbnail twice
                        thumb = dbThumbs.idThumbs.FirstOrDefault(x => x.ImageGUID == imageGuid && x.idType == type);
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

                //        log.LogInformation(String.Format("Client {0}:{1} called GetImageThumbnail with type: {2} imageGuid: {3} (returned resizedImageStream)",
                //            HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort, type, imageGuid));
                //    }
                //    else
                //    {
                //        log.LogInformation(String.Format("Client {0}:{1} called GetImageThumbnail with type: {2} imageGuid: {3} (returned null)",
                //            HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort, type, imageGuid));
                //    }
                //}
                //else
                //{
                //    imageStream = new MemoryStream(thumb.idThumb);

                //    //if (IsRequestRest()) ??
                //    HttpContext.Response.ContentType = "image/jpeg";

                //    log.LogInformation(String.Format("Client {0}:{1} called GetImageThumbnail with type: {2} imageGuid: {3} (returned imageStream)",
                //        HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort, type, imageGuid));
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
    }
}
