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
using Serilog.Context;
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
    /// <summary>
    /// Controller for video streaming
    /// </summary>
    [Route("api/[controller]/[action]")]
    public class MediaController : Controller
    {
        private readonly IDImagerDB db;
        private readonly ILogger<ValuesController> logger;
        private readonly IDiagnosticContext diagnosticContext;
        private readonly ServiceSettings serviceSettings;

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="db">IDImagerDB</param>
        /// <param name="serviceSettings">ServiceSettings</param>
        /// <param name="logger">Logger</param>
        /// <param name="diagnosticContext">Logger diagnostic context</param>
        public MediaController(IDImagerDB db, ServiceSettings serviceSettings,
            ILogger<ValuesController> logger, IDiagnosticContext diagnosticContext)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(logger));
            this.serviceSettings = serviceSettings ?? throw new ArgumentNullException(nameof(serviceSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        }

        /// <summary>
        /// Returns an http video stream
        /// </summary>
        /// <param name="guid">Image guid of video to stream</param>
        /// <param name="videosize">Optional videosize parameter for video transcoding</param>
        /// <returns>Http video stream</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Play(string guid, string videosize)
        {
            using (LogContext.PushProperty(nameof(guid), guid))
            using (LogContext.PushProperty(nameof(videosize), videosize))
            {
                try
                {
                    if (string.IsNullOrEmpty(guid)) return BadRequest("Missing guid parameter");

                    idCatalogItem catalogItem = await db.idCatalogItem.Include(x => x.idFilePath).SingleAsync(x => x.GUID.Equals(guid));
                    string strFilePath = StaticFunctions.GetImageFilePath(catalogItem, serviceSettings.FilePathReplace);

                    string mimeType = GetMimeNameFromExt(catalogItem.FileName);

                    if (videosize != null && !videosize.Equals("Original"))
                    {
                        if (string.IsNullOrEmpty(serviceSettings.TranscodeDirectory))
                            return BadRequest("Missing TranscodeDirectory setting");

                        catalogItem.GetHeightAndWidth(out int originalVideoWidth, out int originalVideoHeight);

                        string strTranscodeFilePath = await StaticFunctions.TranscodeVideo(strFilePath, guid,
                            serviceSettings.TranscodeDirectory, videosize, originalVideoWidth, originalVideoHeight);

                        FileInfo transcodeFileInfo = new FileInfo(strTranscodeFilePath);

                        if (!transcodeFileInfo.Exists)
                            throw new Exception("Transcoding failed, file does not exist.");

                        if (transcodeFileInfo.Length == 0)
                            throw new Exception("Transcoding failed, file size is zero.");

                        strFilePath = strTranscodeFilePath;
                        mimeType = "video/mp4";
                    }

                    Stream inputStream = StaticFunctions.GetImageFileStream(strFilePath);
                    FileStreamResult fileStreamResult = new FileStreamResult(inputStream, mimeType)
                    {
                        EnableRangeProcessing = true,
                        
                    };

                    return fileStreamResult;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    return BadRequest(ex.ToString());
                }
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
