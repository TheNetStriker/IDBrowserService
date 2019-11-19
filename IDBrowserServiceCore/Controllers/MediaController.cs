using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class MediaController : Controller
    {
        private readonly IDImagerDB db;
        private readonly ILogger logger;
        private readonly ServiceSettings serviceSettings;

        public MediaController(IDImagerDB db, IOptions<ServiceSettings> serviceSettings)
        {
            this.db = db;
            this.serviceSettings = serviceSettings.Value;
            this.logger = Log.Logger.ForContext<MediaController>();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Play(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                    return BadRequest("Missing guid parameter");

                idCatalogItem catalogItem = await db.idCatalogItem.Include(x => x.idFilePath).SingleAsync(x => x.GUID.Equals(guid));

                String strFilePath = StaticFunctions.GetImageFilePath(catalogItem, serviceSettings.FilePathReplace);
                Stream inputStream = StaticFunctions.GetImageFileStream(strFilePath);
                string mimeType = GetMimeNameFromExt(catalogItem.FileName);

                FileStreamResult fileStreamResult = new FileStreamResult(inputStream, mimeType)
                {
                    EnableRangeProcessing = true
                };

                return fileStreamResult;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return BadRequest(ex.ToString());
            }
        }

        private string GetMimeNameFromExt(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out string contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}
