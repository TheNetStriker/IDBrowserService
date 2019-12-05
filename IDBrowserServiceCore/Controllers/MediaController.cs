using FFmpeg.NET;
using FFmpeg.NET.Enums;
using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ValuesController> logger;
        private readonly IDiagnosticContext diagnosticContext;
        private readonly IConfiguration configuration;
        private readonly ServiceSettings serviceSettings;

        private readonly string ffmpegPath;

        public MediaController(IDImagerDB db, IConfiguration configuration, IOptions<ServiceSettings> serviceSettings,
            ILogger<ValuesController> logger, IDiagnosticContext diagnosticContext)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(logger));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.serviceSettings = serviceSettings.Value ?? throw new ArgumentNullException(nameof(serviceSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));

            this.ffmpegPath = configuration.GetValue<string>("FFMpegPath");
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Play(string guid, string videosize)
        {
            try
            {
                if (string.IsNullOrEmpty(guid)) return BadRequest("Missing guid parameter");

                diagnosticContext.Set(nameof(guid), guid);
                diagnosticContext.Set(nameof(videosize), videosize);

                idCatalogItem catalogItem = await db.idCatalogItem.Include(x => x.idFilePath).SingleAsync(x => x.GUID.Equals(guid));
                String strFilePath = StaticFunctions.GetImageFilePath(catalogItem, serviceSettings.FilePathReplace);
                
                string mimeType = GetMimeNameFromExt(catalogItem.FileName);

                if (videosize != null && !videosize.Equals("Original"))
                {
                    if (string.IsNullOrEmpty(serviceSettings.TranscodeDirectory))
                        return BadRequest("Missing TranscodeDirectory setting");

                    string strTranscodeFilePath = await StaticFunctions.TranscodeVideo(strFilePath, guid,
                        serviceSettings.TranscodeDirectory, ffmpegPath, videosize);

                    strFilePath = strTranscodeFilePath;
                    mimeType = "video/mp4";
                }

                Stream inputStream = StaticFunctions.GetImageFileStream(strFilePath);
                FileStreamResult fileStreamResult = new FileStreamResult(inputStream, mimeType)
                {
                    EnableRangeProcessing = true
                };

                return fileStreamResult;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
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
