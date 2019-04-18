using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Code;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using IDBrowserServiceCore.Settings;

namespace IDBrowserServiceCore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class MediaController : Controller
    {
        private readonly IDImagerDB db;
        private readonly ILogger log;
        private readonly ServiceSettings serviceSettings;

        public MediaController(IDImagerDB db, IOptions<ServiceSettings> serviceSettings, ILoggerFactory DepLoggerFactory)
        {
            this.db = db;
            this.serviceSettings = serviceSettings.Value;

            if (log == null)
                log = DepLoggerFactory.CreateLogger("Controllers.MediaController");
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
                log.LogError(ex.ToString());
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
