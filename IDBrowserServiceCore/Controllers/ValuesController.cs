﻿using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Code.XmpRecipe;
using IDBrowserServiceCore.Data;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using IDBrowserServiceCore.Services;
using IDBrowserServiceCore.Settings;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace IDBrowserServiceCore.Controllers
{
    /// <summary>
    /// Controller for Photosupreme values
    /// </summary>
    [Route("api/[controller]/[action]")]
    [Route("Service.svc/[action]")] //Compatibility to old service
    [Authorize]
    public class ValuesController : Controller
    {
        private readonly ILogger<ValuesController> logger;
        private readonly IDiagnosticContext diagnosticContext;
        private readonly ServiceSettings serviceSettings;
        private readonly TransactionOptions readUncommittedTransactionOptions;
        private readonly IDImagerDB db;
        private readonly IDImagerThumbsDB dbThumbs;
        private readonly IDatabaseCache databaseCache;

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="db">IDImagerDB</param>
        /// <param name="dbThumbs">IDImagerThumbsDB</param>
        /// <param name="serviceSettings">ServiceSettings</param>
        /// <param name="logger">Logger</param>
        /// <param name="diagnosticContext">Logger diagnostic context</param>
        /// <param name="databaseCache">Database cache</param>
        public ValuesController(IDImagerDB db, IDImagerThumbsDB dbThumbs, ServiceSettings serviceSettings,
            ILogger<ValuesController> logger, IDiagnosticContext diagnosticContext, IDatabaseCache databaseCache)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.dbThumbs = dbThumbs ?? throw new ArgumentNullException(nameof(dbThumbs));
            this.serviceSettings = serviceSettings ?? throw new ArgumentNullException(nameof(serviceSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
            this.databaseCache = databaseCache ?? throw new ArgumentNullException(nameof(databaseCache));

            readUncommittedTransactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            };
        }

        /// <summary>
        /// Returns the image categories
        /// </summary>
        /// <returns>Image categories</returns>
        [HttpGet()]
        public async Task<ActionResult<List<ImageProperty>>> GetImageProperties()
        {
            return await GetImageProperties(null);
        }

        /// <summary>
        /// Query image properties
        /// </summary>
        /// <param name="guid">Guid of the image properties to return.</param>
        /// <returns>Image properties</returns>
        [HttpGet("{guid}")]
        public async Task<ActionResult<List<ImageProperty>>> GetImageProperties(string guid = null)
        {
            using (LogContext.PushProperty(nameof(guid), guid))
            {
                try
                {
                    if (string.IsNullOrEmpty(guid))
                        LogHttpConnection("GetImageProperties");
                    else
                        LogHttpConnection(string.Format("GetImageProperties with guid: {0}", guid));

                    List<ImageProperty> listImageProperty = null;

                    if (guid == null)
                    {
                        if (serviceSettings.EnableDatabaseCache)
                        {
                            listImageProperty = await databaseCache.GetVPropCategoryCacheAsync();
                        }

                        if (listImageProperty == null)
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
                    }
                    else
                    {
                        if (serviceSettings.EnableDatabaseCache)
                        {
                            listImageProperty = await databaseCache.GetVPropCacheAsync();
                            
                            if (listImageProperty != null)
                            {
                                listImageProperty = listImageProperty
                                    .Where(x => x.ParentGUID.Equals(guid))
                                    .ToList();
                            }
                        }

                        if (listImageProperty == null)
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
                    }

                    return listImageProperty;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns image property or image category thumbnail
        /// </summary>
        /// <param name="guid">Guid of the image property</param>
        /// <param name="isCategory">True if the property is an image category</param>
        /// <returns>image property or image category thumbnail</returns>
        [HttpGet("{guid}/{isCategory}")]
        public async Task<ActionResult<Stream>> GetImagePropertyThumbnail([Required] string guid, [Required] string isCategory)
        {
            using (LogContext.PushProperty(nameof(guid), guid))
            using (LogContext.PushProperty(nameof(isCategory), isCategory))
            {
                try
                {
                    if (guid is null) return StaticFunctions.BadRequestArgumentNull(nameof(guid));
                    if (isCategory is null) return StaticFunctions.BadRequestArgumentNull(nameof(isCategory));

                    return await GetImagePropertyThumbnailStream(guid, isCategory);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns resized image property or image category thumbnail
        /// </summary>
        /// <param name="guid">Guid of the image property</param>
        /// <param name="isCategory">True if the property is an image category</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns>Resized image property or image category thumbnail</returns>
        [HttpGet("{guid}/{isCategory}/{width}/{height}")]
        public async Task<ActionResult<Stream>> GetResizedImagePropertyThumbnail([Required] string guid, [Required] string isCategory, string width, string height)
        {
            using (LogContext.PushProperty(nameof(guid), guid))
            using (LogContext.PushProperty(nameof(isCategory), isCategory))
            using (LogContext.PushProperty(nameof(width), width))
            using (LogContext.PushProperty(nameof(height), height))
            {
                try
                {
                    if (guid is null) return StaticFunctions.BadRequestArgumentNull(nameof(guid));
                    if (isCategory is null) return StaticFunctions.BadRequestArgumentNull(nameof(isCategory));

                    return await GetImagePropertyThumbnailStream(guid, isCategory, width, height);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        private async Task<ActionResult<Stream>> GetImagePropertyThumbnailStream(string guid, string isCategory, string width = null, string height = null)
        {
            byte[] idImage;

            if (guid is null) return StaticFunctions.BadRequestArgumentNull(nameof(guid));
            if (isCategory is null) return StaticFunctions.BadRequestArgumentNull(nameof(isCategory));

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

                return BadRequest("Image not found.");
            }
            else
            {               
                imageStream = new MemoryStream(idImage);
                LogHttpConnection(string.Format("GetImagePropertyThumbnail with guid: {0} isCategory: {1}",
                    guid, isCategory));
            }

            FileStreamResult imageStreamResult = new FileStreamResult(imageStream, "image/jpeg")
            {
                EnableRangeProcessing = true,
            };

            return imageStreamResult;
        }

        /// <summary>
        /// Returns catalog items of image property guid. 
        /// </summary>
        /// <param name="orderDescending">Order catalog items descending</param>
        /// <param name="propertyGuid">Image property guid</param>
        /// <param name="includeSubitems">Include catalog items from sub properties</param>
        /// <returns>Catalog items</returns>
        [HttpGet("{orderDescending}/{propertyGuid}")]
        public async Task<ActionResult<List<CatalogItem>>> GetCatalogItems([Required] string orderDescending, [Required] string propertyGuid,
            bool includeSubitems = false)
        {
            using (LogContext.PushProperty(nameof(orderDescending), orderDescending))
            using (LogContext.PushProperty(nameof(propertyGuid), propertyGuid))
            {
                try
                {
                    if (orderDescending is null) return StaticFunctions.BadRequestArgumentNull(nameof(orderDescending));
                    if (propertyGuid is null) return StaticFunctions.BadRequestArgumentNull(nameof(propertyGuid));

                    List<CatalogItem> catalogItems = null;

                    List<string> propertyGuids = new();

                    propertyGuids.Add(propertyGuid);

                    if (includeSubitems)
                    {
                        await AddSubpropertyGuids(propertyGuid, propertyGuids);
                    }

                    var query = from tbl in db.idCatalogItemDefinition
                                where propertyGuids.Contains(tbl.GUID)
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
                            FilePath = x.idCatalogItem.idCache_FilePath.FilePath,
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
                    logger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Add subfolder file path guids to supplied list.
        /// </summary>
        /// <param name="propertyGuid">File path guid to query recursive.</param>
        /// <param name="propertyGuids">List to add the guids.</param>
        private async Task AddSubpropertyGuids(string propertyGuid, List<string> propertyGuids)
        {
            var subfolderQuery = from tbl in db.idProp
                                 where tbl.ParentGUID == propertyGuid
                                 select tbl;

            foreach (var prop in await subfolderQuery.ToListAsync())
            {
                if (!propertyGuids.Contains(prop.GUID))
                {
                    propertyGuids.Add(prop.GUID);
                    await AddSubpropertyGuids(prop.GUID, propertyGuids);
                }
            }
        }

        /// <summary>
        /// Returns catalog items of image file path. 
        /// </summary>
        /// <param name="orderDescending">Order catalog items descending</param>
        /// <param name="filePathGuid">File path guid</param>
        /// <param name="includeSubitems">Include items in subfolders</param>
        /// <returns>Catalog items</returns>
        [HttpGet("{orderDescending}/{filePathGuid}")]
        public async Task<ActionResult<List<CatalogItem>>> GetCatalogItemsByFilePath([Required] string orderDescending, [Required] string filePathGuid,
            bool includeSubitems = false)
        {
            using (LogContext.PushProperty(nameof(orderDescending), orderDescending))
            using (LogContext.PushProperty(nameof(filePathGuid), filePathGuid))
            {
                try
                {
                    if (orderDescending is null) return StaticFunctions.BadRequestArgumentNull(nameof(orderDescending));
                    if (filePathGuid is null) return StaticFunctions.BadRequestArgumentNull(nameof(filePathGuid));

                    List<CatalogItem> catalogItems = null;

                    List<string> filePathGuids = new();

                    filePathGuids.Add(filePathGuid);

                    if (includeSubitems)
                    {
                        await AddSubfolderFilePathGuids(filePathGuid, filePathGuids);
                    }

                    var query = from tbl in db.idCatalogItem
                                where filePathGuids.Contains(tbl.PathGUID)
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
                            FilePath = x.idCache_FilePath.FilePath,
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
                    logger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Add subfolder file path guids to supplied list.
        /// </summary>
        /// <param name="filePathGuid">File path guid to query recursive.</param>
        /// <param name="filePathGuids">List to add the guids.</param>
        private async Task AddSubfolderFilePathGuids(string filePathGuid, List<string> filePathGuids)
        {
            var subfolderQuery = from tbl in db.idFilePath
                                 where tbl.ParentGUID == filePathGuid
                                 select tbl;

            foreach (var filePath in await subfolderQuery.ToListAsync())
            {
                if (!filePathGuids.Contains(filePath.guid))
                {
                    filePathGuids.Add(filePath.guid);
                    await AddSubfolderFilePathGuids(filePath.guid, filePathGuids);
                }
            }
        }

        /// <summary>
        /// Returns image thumbnail for image guid.
        /// </summary>
        /// <param name="type">Thumbnail size</param>
        /// <param name="imageGuid">Image guid</param>
        /// <returns>Image thumbnail</returns>
        [HttpGet("{type}/{imageGuid}")]
        public async Task<ActionResult<Stream>> GetImageThumbnail([Required] string type, [Required] string imageGuid)
        {
            using (LogContext.PushProperty(nameof(type), type))
            using (LogContext.PushProperty(nameof(imageGuid), imageGuid))
            {
                try
                {
                    if (type is null) return StaticFunctions.BadRequestArgumentNull(nameof(type));
                    if (imageGuid is null) return StaticFunctions.BadRequestArgumentNull(nameof(imageGuid));

                    return await GetImageThumbnailStream(type, imageGuid);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        private async Task<ActionResult<Stream>> GetImageThumbnailStream(string type, string imageGuid)
        {
            if (type is null) return StaticFunctions.BadRequestArgumentNull(nameof(type));
            if (imageGuid is null) return StaticFunctions.BadRequestArgumentNull(nameof(imageGuid));

            if (type != "T" && type != "R" && type != "M")
                return BadRequest("Unsupported image type");

            idCatalogItem catalogItem = null;
            Stream imageStream = null;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                catalogItem = await db.idCatalogItem.Include(x => x.idFilePath).SingleOrDefaultAsync(x => x.GUID == imageGuid);

                if (catalogItem == null)
                    return BadRequest("CatalogItem not found");

                if (type == "R" && catalogItem.idHasRecipe == 0)
                    return BadRequest("Image has no recipe");

                //Check if CatalogItem has a recipe, if yes try to get the recipe image
                if (type == "M" && catalogItem.idHasRecipe > 0)
                    type = "R";

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

                    // ToDo: Externe Image Library implementieren für das.
                    if (thumb == null && serviceSettings.CreateThumbnails)
                    {
                        SaveImageThumbnailResult result = await StaticFunctions.SaveImageThumbnail(catalogItem,
                            db, dbThumbs, new List<string>() { type }, serviceSettings);

                        foreach (Exception ex in result.Exceptions)
                            logger.LogError(ex.ToString());

                        if (result.ImageStreams.Count > 0)
                        {
                            imageStream = result.ImageStreams.First();

                            LogHttpConnection(string.Format("GetImageThumbnail with type: {0} imageGuid: {1} (returned resizedImageStream)",
                                type, imageGuid));
                        }
                        else
                        {
                            LogHttpConnection(string.Format("GetImageThumbnail with type: {0} imageGuid: {1} (returned null)",
                                type, imageGuid));

                            return BadRequest("Thumbnail generation failed.");
                        }
                    }
                    else
                    {
                        imageStream = new MemoryStream(thumb.idThumb);

                        LogHttpConnection(string.Format("GetImageThumbnail with type: {0} imageGuid: {1} (returned imageStream)",
                            type, imageGuid));
                    }

                    scope2.Complete();
                }

                scope.Complete();
            }

            FileStreamResult imageStreamResult = new FileStreamResult(imageStream, "image/jpeg")
            {
                EnableRangeProcessing = true,
            };

            return imageStreamResult;
        }

        /// <summary>
        /// Retuns full size image by image guid and applies image recipies.
        /// </summary>
        /// <param name="imageGuid">Image guid</param>
        /// <returns>Full size image</returns>
        [HttpGet("{imageGuid}")]
        public async Task<ActionResult<Stream>> GetImage([Required] string imageGuid)
        {
            using (LogContext.PushProperty(nameof(imageGuid), imageGuid))
            {
                ActionResult<Stream> getImageStreamResult = null;
                try
                {
                    if (imageGuid is null) return StaticFunctions.BadRequestArgumentNull(nameof(imageGuid));

                    LogHttpConnection(string.Format("GetImage with imageGuid: {0}", imageGuid));

                    return await GetImageStream(imageGuid, "image/jpeg");
                }
                catch (Exception ex)
                {
                    if (getImageStreamResult != null && getImageStreamResult.Value != null) { getImageStreamResult.Value.Close(); }
                    logger.LogError(ex.ToString());
                    throw;
                }
            }  
        }

        /// <summary>
        /// Returns resized image from original file and applies image recipies.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="imageGuid">Image guid</param>
        /// <returns>Resized image</returns>
        [HttpGet("{width}/{height}/{imageGuid}")]
        public async Task<ActionResult<Stream>> GetResizedImage([Required] string width, [Required] string height, [Required] string imageGuid)
        {
            using (LogContext.PushProperty(nameof(width), width))
            using (LogContext.PushProperty(nameof(height), height))
            using (LogContext.PushProperty(nameof(imageGuid), imageGuid))
            {
                ActionResult<Stream> getImageStreamResult = null;
                try
                {
                    if (width is null) return StaticFunctions.BadRequestArgumentNull(nameof(width));
                    if (height is null) return StaticFunctions.BadRequestArgumentNull(nameof(height));
                    if (imageGuid is null) return StaticFunctions.BadRequestArgumentNull(nameof(imageGuid));

                    LogHttpConnection(string.Format("GetResizedImage with width: {0} height {1} imageGuid: {2}",
                        width, height, imageGuid));

                    return await GetImageStream(imageGuid, "image/jpeg", width, height);
                }
                catch (Exception ex)
                {
                    if (getImageStreamResult != null && getImageStreamResult.Value != null) { getImageStreamResult.Value.Close(); }
                    logger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        private async Task<ActionResult<Stream>> GetImageStream([Required] string imageGuid, string mimeType, string width = null, string height = null)
        {
            if (imageGuid is null) return StaticFunctions.BadRequestArgumentNull(nameof(imageGuid));

            idCatalogItem catalogItem = null;

            catalogItem = await db.idCatalogItem
                .Include(x => x.idFilePath)
                .Include(x => x.idCache_FilePath)
                .SingleOrDefaultAsync(x => x.GUID == imageGuid);

            if (catalogItem == null)
                return BadRequest("CatalogItem not found");

            string strImageFilePath = StaticFunctions.GetImageFilePath(catalogItem, serviceSettings.FilePathReplace);
            Stream imageStream = StaticFunctions.GetImageFileStream(strImageFilePath);

            XmpRecipeContainer xmpRecipeContainer = null;
            try
            {
                System.Xml.Linq.XDocument recipeXDocument = await StaticFunctions.GetRecipeXDocument(db, catalogItem);
                xmpRecipeContainer = XmpRecipeHelper.ParseXmlRecepie(recipeXDocument);
            }
            catch (Exception ex)
            {
                logger.LogError(String.Format("Error in 'GetImageStream' when applying recipe on imageGuid {0}: {1}",
                    catalogItem.GUID, ex.ToString()));
            }

            MagickImage image = new MagickImage(imageStream);

            if ((width != null && height != null) || (xmpRecipeContainer != null && xmpRecipeContainer.HasValues))
            {
                image.AutoOrient();

                if (xmpRecipeContainer != null && xmpRecipeContainer.HasValues)
                    XmpRecipeHelper.ApplyXmpRecipe(xmpRecipeContainer, image);

                if (width != null && height != null)
                {
                    uint intWidth = uint.Parse(width);
                    uint intHeight = uint.Parse(height);

                    if (image.Width > intWidth && image.Height > intHeight)
                    {
                        image.Resize(intWidth, intHeight);
                    }
                }

                imageStream = new MemoryStream();
                image.Write(imageStream);

                imageStream.Position = 0;
            }
            else
            {
                // If we don't resize the image, check if it needs to be re-oriented.
                var exifValue = image
                    .GetExifProfile()
                    .GetValue(ExifTag.Orientation);

                if (exifValue != null && exifValue.Value > 0)
                {
                    image.AutoOrient();

                    imageStream = new MemoryStream();
                    image.Write(imageStream);
                    imageStream.Position = 0;
                }
            }

            FileStreamResult imageStreamResult = new FileStreamResult(imageStream, mimeType)
            {
                EnableRangeProcessing = true,
            };

            return imageStreamResult;
        }

        /// <summary>
        /// Returns image infos for image guid.
        /// </summary>
        /// <param name="imageGuid">Image guid</param>
        /// <returns>Image infos</returns>
        [HttpGet("{imageGuid}")]
        public async Task<ActionResult<ImageInfo>> GetImageInfo([Required] string imageGuid)
        {
            using (LogContext.PushProperty(nameof(imageGuid), imageGuid))
            {
                try
                {
                    if (imageGuid is null) return StaticFunctions.BadRequestArgumentNull(nameof(imageGuid));

                    var queryXMP = from tbl in db.idSearchData
                                   where tbl.RelatedGUID == imageGuid
                                   && tbl.ContentType.Equals("XMP")
                                   orderby tbl.ContentGroup
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
                    logger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns random image guids.
        /// </summary>
        /// <param name="imageFileExtensions">File extensions to include.</param>
        /// <param name="count">Count of images guid's to return.</param>
        /// <returns>Random image guids.</returns>
        [HttpGet()]
        public async Task<ActionResult<List<String>>> GetRandomImageGuids([Required] List<string> imageFileExtensions, int count = 5)
        {
            using (LogContext.PushProperty(nameof(imageFileExtensions), imageFileExtensions))
            {
                try
                {
                    if (imageFileExtensions is null) return StaticFunctions.BadRequestArgumentNull(nameof(imageFileExtensions));

                    List<String> randomImageGuids;

                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                        readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var queryRandom = from x in db.idCatalogItem
                                          where imageFileExtensions.Contains(x.idFileType)
                                          orderby Guid.NewGuid()
                                          select x.GUID;
                        randomImageGuids = await queryRandom.Take(count).ToListAsync();
                        scope.Complete();
                    }

                    LogHttpConnection("GetRandomImageGuids");
                    return randomImageGuids;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    throw;
                }
            } 
        }

        /// <summary>
        /// Returns raw source file by image guid.
        /// </summary>
        /// <param name="imageGuid">Image guid</param>
        /// <returns>Raw image file</returns>
        [HttpGet("{imageGuid}")]
        public async Task<ActionResult<Stream>> GetFile(string imageGuid)
        {
            using (LogContext.PushProperty(nameof(imageGuid), imageGuid))
            {
                Stream fileStream = null;
                try
                {
                    if (imageGuid is null) return StaticFunctions.BadRequestArgumentNull(nameof(imageGuid));

                    LogHttpConnection(string.Format("GetFile with imageGuid: {0}", imageGuid));

                    idCatalogItem catalogItem;

                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                        readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                    {
                        catalogItem = await db.idCatalogItem
                            .Include(x => x.idFilePath)
                            .Include(x => x.idCache_FilePath)
                            .SingleOrDefaultAsync(x => x.GUID == imageGuid);
                        scope.Complete();
                    }

                    if (catalogItem == null)
                        return BadRequest("CatalogItem not found");

                    string strImageFilePath = StaticFunctions.GetImageFilePath(catalogItem, serviceSettings.FilePathReplace);
                    fileStream = StaticFunctions.GetImageFileStream(strImageFilePath);

                    FileStreamResult fileStreamResult = new FileStreamResult(fileStream, "application/octet-stream")
                    {
                        EnableRangeProcessing = true,
                        FileDownloadName = catalogItem.FileName
                    };

                    return fileStreamResult;
                }
                catch (Exception ex)
                {
                    if (fileStream != null) { fileStream.Close(); }
                    logger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Search for image properties
        /// </summary>
        /// <param name="searchString">Search text</param>
        /// <returns>Image properties</returns>
        [HttpGet("{searchString}")]
        public async Task<ActionResult<List<ImagePropertyRecursive>>> SearchImageProperties(string searchString)
        {
            using (LogContext.PushProperty(nameof(searchString), searchString))
            {
                try
                {
                    if (searchString is null) return StaticFunctions.BadRequestArgumentNull(nameof(searchString));

                    var queryProperties = from tbl in db.idProp
                                          where tbl.PropName.ToLower().Contains(searchString.ToLower())
                                          orderby tbl.PropName
                                          select tbl;

                    List<ImagePropertyRecursive> listImagePropertyRecursive = await GetImagePropertyRecursive(queryProperties);

                    LogHttpConnection(string.Format("SearchImagePropertiesSoap with searchString: {0}", searchString));
                    return listImagePropertyRecursive;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    throw;
                }
            } 
        }

        /// <summary>
        /// Returns image properties by catalog item guid.
        /// </summary>
        /// <param name="catalogItemGUID">Catalog item guid</param>
        /// <returns>Image properties</returns>
        [HttpGet("{catalogItemGUID}")]
        public async Task<ActionResult<List<ImagePropertyRecursive>>> GetCatalogItemImageProperties(string catalogItemGUID)
        {
            using (LogContext.PushProperty(nameof(catalogItemGUID), catalogItemGUID))
            {
                try
                {
                    if (catalogItemGUID is null) return StaticFunctions.BadRequestArgumentNull(nameof(catalogItemGUID));

                    var queryCatalogItemDefinition = from tbl in db.idCatalogItemDefinition
                                                     where tbl.CatalogItemGUID == catalogItemGUID
                                                     select tbl.GUID;

                    List<String> propertyGuids;
                    List<ImagePropertyRecursive> listImagePropertyRecursive;

                    propertyGuids = await queryCatalogItemDefinition.ToListAsync();

                    var queryProperties = from tbl in db.idProp
                                          where propertyGuids.Contains(tbl.GUID)
                                          orderby tbl.PropName
                                          select tbl;

                    listImagePropertyRecursive = await GetImagePropertyRecursive(queryProperties);

                    LogHttpConnection(string.Format("GetCatalogItemImageProperties with catalogItemGUID: {0}",
                        catalogItemGUID));
                    return listImagePropertyRecursive;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    throw;
                }
            } 
        }

        private async Task<List<ImagePropertyRecursive>> GetImagePropertyRecursive(IQueryable<idProp> query)
        {
            List<ImagePropertyRecursive> listImagePropertyRecursive = new();
            foreach (idProp row in query.ToList())
            {
                String parentGuid = row.ParentGUID;
                List<String> parentProperties = new();

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

        /// <summary>
        /// Adds an image property to a catalog item.
        /// </summary>
        /// <param name="propertyGuid">Image property to add</param>
        /// <param name="catalogItemGUID">Catalog item to add the property to</param>
        /// <returns>"OK" if property was added.</returns>
        [HttpGet("{propertyGuid}/{catalogItemGUID}")]
        public async Task<ActionResult<string>> AddCatalogItemDefinition(string propertyGuid, string catalogItemGUID)
        {
            using (LogContext.PushProperty(nameof(propertyGuid), propertyGuid))
            using (LogContext.PushProperty(nameof(catalogItemGUID), catalogItemGUID))
            {
                try
                {
                    if (propertyGuid is null) return StaticFunctions.BadRequestArgumentNull(nameof(propertyGuid));
                    if (catalogItemGUID is null) return StaticFunctions.BadRequestArgumentNull(nameof(catalogItemGUID));

                    if (await db.idProp.Where(x => x.GUID == propertyGuid).CountAsync() == 0)
                        return BadRequest("Image property does not exist");

                    var query = from tbl in db.idCatalogItemDefinition
                                where tbl.GUID == propertyGuid & tbl.CatalogItemGUID == catalogItemGUID
                                select tbl;

                    if (await query.CountAsync() > 0)
                        return BadRequest("CatalogItemDefinition already exists");

                    idCatalogItem currentIdCatalogItem = await db.idCatalogItem.SingleAsync(x => x.GUID == catalogItemGUID);
                    idImageVersion currentIdImageVersion = await db.idImageVersion.SingleOrDefaultAsync(x => x.MainImageGUID == catalogItemGUID);

                    idCatalogItemDefinition newIdCatalogItemDefinition = new idCatalogItemDefinition
                    {
                        GUID = propertyGuid,
                        CatalogItemGUID = catalogItemGUID,
                        idAssigned = DateTime.Now
                    };

                    db.idCatalogItemDefinition.Add(newIdCatalogItemDefinition);

                    currentIdCatalogItem.idInSync >>= 2;
                    if (currentIdImageVersion != null)
                        currentIdImageVersion.idInSync >>= 2;

                    await db.SaveChangesAsync();

                    LogHttpConnection(string.Format("AddCatalogItemDefinition with propertyGuid: {0} and catalogItemGUID {1}",
                        propertyGuid, catalogItemGUID));
                    return "OK";
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes an image propery of a catalog item.
        /// </summary>
        /// <param name="propertyGuid">Property guid to delete</param>
        /// <param name="catalogItemGUID">Catalog item guid to remove the property from</param>
        /// <returns>"OK" if property was added.</returns>
        [HttpGet("{propertyGuid}/{catalogItemGUID}")]
        public async Task<ActionResult<string>> DeleteCatalogItemDefinition(string propertyGuid, string catalogItemGUID)
        {
            using (LogContext.PushProperty(nameof(propertyGuid), propertyGuid))
            using (LogContext.PushProperty(nameof(catalogItemGUID), catalogItemGUID))
            {
                try
                {
                    if (propertyGuid is null) return StaticFunctions.BadRequestArgumentNull(nameof(propertyGuid));
                    if (catalogItemGUID is null) return StaticFunctions.BadRequestArgumentNull(nameof(catalogItemGUID));

                    idCatalogItemDefinition currentIdCatalogItemDefinition = await db.idCatalogItemDefinition
                        .SingleAsync(x => x.GUID == propertyGuid && x.CatalogItemGUID == catalogItemGUID);
                    idCatalogItem currentIdCatalogItem = await db.idCatalogItem
                        .SingleAsync(x => x.GUID == catalogItemGUID);
                    idImageVersion currentIdImageVersion = await db.idImageVersion
                        .SingleOrDefaultAsync(x => x.MainImageGUID == catalogItemGUID);

                    db.idCatalogItemDefinition.Remove(currentIdCatalogItemDefinition);

                    currentIdCatalogItem.idInSync >>= 2;
                    if (currentIdImageVersion != null)
                        currentIdImageVersion.idInSync >>= 2;

                    await db.SaveChangesAsync();

                    LogHttpConnection(string.Format("DeleteCatalogItemDefinition with propertyGuid: {0} and catalogItemGUID {1}",
                        propertyGuid, catalogItemGUID));
                    return "OK";
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Adds an new image property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="parentGUID">Parent image property or image category guid</param>
        /// <returns>"OK" if property was added.</returns>
        [HttpGet("{propertyName}/{parentGUID}")]
        [Produces("text/plain")]
        public async Task<ActionResult<string>> AddImageProperty(string propertyName, string parentGUID)
        {
            using (LogContext.PushProperty(nameof(propertyName), propertyName))
            using (LogContext.PushProperty(nameof(parentGUID), parentGUID))
            {
                try
                {
                    if (propertyName is null) return StaticFunctions.BadRequestArgumentNull(nameof(propertyName));
                    if (parentGUID is null) return StaticFunctions.BadRequestArgumentNull(nameof(parentGUID));

                    idProp parentIdProp = await db.idProp.SingleOrDefaultAsync(x => x.GUID.Equals(parentGUID));
                    int? parentRgt;

                    if (parentIdProp == null)
                        parentRgt = await db.idProp.Where(x => x.ParentGUID.Equals(parentGUID)).MaxAsync(x => x.rgt);
                    else
                        parentRgt = parentIdProp.rgt;

                    idUser user = await db.idUser.FirstAsync();
                    DateTime dtNow = DateTime.Now;
                    String newIdPropGuid = Guid.NewGuid().ToString().ToUpper();

                    while (await db.idProp.Where(x => x.GUID == newIdPropGuid).CountAsync() > 0)
                    {
                        newIdPropGuid = Guid.NewGuid().ToString().ToUpper();
                    }

                    String newIdSearchDataGuid = Guid.NewGuid().ToString().ToUpper();

                    while (await db.idSearchData.Where(x => x.GUID == newIdSearchDataGuid).CountAsync() > 0)
                    {
                        newIdSearchDataGuid = Guid.NewGuid().ToString().ToUpper();
                    }

                    //Update nested set rgt
                    var queryUpdateNestedSetRgt = from tbl in db.idProp
                                                  where tbl.rgt >= parentRgt
                                                  select tbl;

                    foreach (idProp row in await queryUpdateNestedSetRgt.ToListAsync())
                    {
                        row.rgt += 2;
                    }

                    //Update nested set lft
                    var queryUpdateNestedSetLft = from tbl in db.idProp
                                                  where tbl.lft > parentRgt
                                                  select tbl;

                    foreach (idProp row in await queryUpdateNestedSetLft.ToListAsync())
                    {
                        row.lft += 2;
                    }

                    idProp newIdProp = new idProp
                    {
                        GUID = newIdPropGuid,
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

                    idCache_Prop newIdCache_Prop = new idCache_Prop
                    {
                        PropGuid = newIdPropGuid,
                        CategoryGuid = parentGUID
                    };
                    db.idCache_Prop.Add(newIdCache_Prop);

                    idSearchData newIdSearchData = new idSearchData
                    {
                        GUID = newIdSearchDataGuid,
                        RelatedGUID = newIdPropGuid,
                        ContentType = "PROP",
                        ContentGroup = "PROPNAME",
                        ContentValue = propertyName
                    };
                    db.idSearchData.Add(newIdSearchData);

                    foreach (string propertyNameSplit in propertyName.Split(' '))
                    {
                        idSearchDataValue newIdSearchDataValue = new idSearchDataValue
                        {
                            GUID = newIdSearchDataGuid,
                            ContentValue = propertyNameSplit
                        };
                        db.idSearchDataValue.Add(newIdSearchDataValue);
                    }

                    await db.SaveChangesAsync();

                    if (serviceSettings.EnableDatabaseCache)
                    {
                        var vPropCache = await databaseCache.GetVPropCacheAsync();

                        if (vPropCache != null)
                        {
                            vPropCache.Add(new ImageProperty
                            {
                                GUID = newIdProp.GUID,
                                ParentGUID = newIdProp.ParentGUID,
                                Name = newIdProp.PropName,
                                ImageCount = 0,
                                SubPropertyCount = 0
                            });

                            vPropCache = vPropCache
                                .OrderBy(x => x.Name)
                                .ToList();

                            await databaseCache.SetVPropCacheAsync(vPropCache);
                        }
                    }

                    LogHttpConnection(string.Format("AddImageProperty with propertyName: {0}, parentGUID {1}, lft {2}, rgt {3}",
                        propertyName, parentGUID, newIdProp.lft, newIdProp.rgt));
                    return "OK";
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns all file paths
        /// </summary>
        /// <returns>All file paths</returns>
        [HttpGet]
        public async Task<ActionResult<List<FilePath>>> GetFilePaths()
        {
            try
            {
                List<FilePath> listFilePaths;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                    readUncommittedTransactionOptions, TransactionScopeAsyncFlowOption.Enabled))
                {
                    var query = from tbl in db.idFilePath
                                where !string.IsNullOrEmpty(tbl.idCache_FilePath.RootGUID)
                                orderby tbl.idCache_FilePath.FilePath
                                select new FilePath
                                {
                                    GUID = tbl.guid,
                                    MediumName = tbl.idMediumInfo.MediumName,
                                    Path = tbl.idCache_FilePath.FilePath,
                                    RootName = tbl.idCache_FilePath.root_idCache_FilePath.FilePath,
                                    ImageCount = tbl.idCatalogItem.Count()
                                };

                    listFilePaths = await query.ToListAsync();
                }

                LogHttpConnection("GetFilePaths");
                return listFilePaths;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Generates token for secured media api
        /// </summary>
        /// <returns>Time limited token</returns>
        [HttpGet("{guid}")]
        public async Task<ActionResult<MediaToken>> GetMediaToken(string guid)
        {
            try
            {
                if (guid == null)
                    return StaticFunctions.BadRequestArgumentNull(nameof(guid));

                idCatalogItem catalogItem = await db.idCatalogItem.Include(x => x.idFilePath).SingleOrDefaultAsync(x => x.GUID.Equals(guid));

                if (catalogItem == null)
                    return BadRequest("Media guid not found!");

                var now = DateTime.UtcNow;
                var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(serviceSettings.TokenSecretKey));

                var claims = new Claim[]
                {
                    new Claim("MediaGuid", guid),
                    new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                var jwtSecurityToken = new JwtSecurityToken(
                    issuer: serviceSettings.TokenIssuer,
                    audience: serviceSettings.TokenAudience,
                    claims: claims,
                    notBefore: now,
                    expires: now.Add(serviceSettings.TokenExpiration),
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
                var encodedJwtSecurityToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

                LogHttpConnection(string.Format("GetMediaToken with guid: {0}", guid));

                return new MediaToken()
                {
                    Token = encodedJwtSecurityToken,
                    ValidFrom = jwtSecurityToken.ValidFrom,
                    ValidTo = jwtSecurityToken.ValidTo
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Returns the version of web service
        /// </summary>
        /// <returns>Version</returns>
        [HttpGet]
        public string GetVersion()
        {
            return System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        private void LogHttpConnection(string callingMethod)
        {
            if (HttpContext != null && HttpContext.Connection != null)
            {
                logger.LogInformation(String.Format("Client {0}:{1} called {2}", 
                    HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort, callingMethod));
            }
            else
            {
                logger.LogInformation(String.Format("Called {0}", callingMethod));
            }
        }
    }
}
