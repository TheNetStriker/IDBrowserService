using IDBrowserServiceCore.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using IDBrowserServiceCore.Data.IDImager;

namespace IDBrowserServiceCore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class MediaController : Controller
    {
        // This will be used in copying input stream to output stream.
        public const int ReadStreamBufferSize = 1024 * 1024;
        // We have a read-only dictionary for mapping file extensions and MIME names. 
        public readonly IReadOnlyDictionary<string, string> MimeNames;

        private IDImagerDB db;
        private ILogger log;

        public MediaController(IDImagerDB db, IConfiguration configuration, ILoggerFactory DepLoggerFactory, IHostingEnvironment DepHostingEnvironment)
        {
            this.db = db;

            if (log == null)
                log = DepLoggerFactory.CreateLogger("Controllers.MediaController");

            //if (db == null)
            //    db = new IDImagerDB(configuration["ConnectionStrings:IDImager"]);

            var mimeNames = new Dictionary<string, string>();

            mimeNames.Add("mp3", "audio/mpeg");    // List all supported media types; 
            mimeNames.Add("mp4", "video/mp4");
            mimeNames.Add("ogg", "application/ogg");
            mimeNames.Add("ogv", "video/ogg");
            mimeNames.Add("oga", "audio/ogg");
            mimeNames.Add("wav", "audio/x-wav");
            mimeNames.Add("webm", "video/webm");

            MimeNames = new ReadOnlyDictionary<string, string>(mimeNames);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Play(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                    return BadRequest("Missing guid parameter");

                idCatalogItem catalogItem = db.idCatalogItem.Include("idFilePath").Single(x => x.GUID.Equals(guid));

                String strFilePath = StaticFunctions.GetImageFilePath(catalogItem);
                FileInfo fileInfo = new FileInfo(strFilePath);
                Stream inputStream = StaticFunctions.GetImageFileStream(strFilePath);
                MediaTypeHeaderValue mimeType = GetMimeNameFromExt(catalogItem.idFileType);

                FileStreamResult fileStreamResult = new FileStreamResult(inputStream, mimeType.ToString());
                fileStreamResult.EnableRangeProcessing = true;

                return fileStreamResult;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return BadRequest(ex.ToString());
            }
        }

        private MediaTypeHeaderValue GetMimeNameFromExt(string ext)
        {
            string value;

            if (MimeNames.TryGetValue(ext.ToLowerInvariant(), out value))
                return new MediaTypeHeaderValue(value);
            else
                return new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);
        }
    }
}
