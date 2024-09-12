using FFmpeg.NET;
using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Context;
using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Controllers
{
    /// <summary>
    /// Controller for video streaming
    /// </summary>
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class MediaController : Controller
    {
        private readonly IDImagerDB db;
        private readonly ILogger<MediaController> logger;
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
            ILogger<MediaController> logger, IDiagnosticContext diagnosticContext)
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
        public async Task<IActionResult> Play([Required] string guid, string videosize)
        {
            if (serviceSettings.DisableInsecureMediaPlayApi)
                return BadRequest("Insecure media api disabled!");

            if (guid is null) return StaticFunctions.BadRequestArgumentNull(nameof(guid));

            return await PlayInternal(guid, videosize);
        }

        /// <summary>
        /// Returns an http video stream secured by a time limited token
        /// </summary>
        /// <param name="token">Authentification token (contains media guid)</param>
        /// <param name="videosize">Optional videosize parameter for video transcoding</param>
        /// <returns>Http video stream</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PlaySecure([Required] string token, string videosize)
        {
            if (token is null) return StaticFunctions.BadRequestArgumentNull(nameof(token));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(serviceSettings.TokenSecretKey)),
                ValidIssuer = serviceSettings.TokenIssuer,
                ValidAudience = serviceSettings.TokenAudience,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                ClaimsPrincipal claimsPrincipal = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out _);

                string mediaGuid = claimsPrincipal.FindFirstValue("MediaGuid");

                if (string.IsNullOrEmpty(mediaGuid))
                {
                    return BadRequest("MediaGuid claim missing!");
                }

                return await PlayInternal(mediaGuid, videosize);
            }
            catch (SecurityTokenException)
            {
                return BadRequest("Token exired!");
            }
        }

        private async Task<IActionResult> PlayInternal(string guid, string videosize)
        {
            using (LogContext.PushProperty(nameof(guid), guid))
            using (LogContext.PushProperty(nameof(videosize), videosize))
            {
                FileInfo transcodeFileInfo = null;

                try
                {
                    if (string.IsNullOrEmpty(guid)) return BadRequest("Missing guid parameter");

                    idCatalogItem catalogItem = await db.idCatalogItem
                        .Include(x => x.idFilePath)
                        .Include(x => x.idCache_FilePath)
                        .SingleAsync(x => x.GUID.Equals(guid));

                    string strFilePath = StaticFunctions.GetImageFilePath(catalogItem, serviceSettings.FilePathReplace);

                    string mimeType = GetMimeNameFromExt(catalogItem.FileName);

                    if (videosize != null && !videosize.Equals("Original"))
                    {
                        if (string.IsNullOrEmpty(serviceSettings.TranscodeDirectory))
                            return BadRequest("Missing TranscodeDirectory setting");

                        catalogItem.GetHeightAndWidth(out int originalVideoWidth, out int originalVideoHeight);

                        Engine ffmpegEngine = StaticFunctions.GetFFmpegEngine();

                        ffmpegEngine.Error += (sender, eventArgs) =>
                        {
                            logger.LogError(eventArgs.Exception.ToString());
                        };

                        string strTranscodeFilePath = StaticFunctions.GetTranscodeFilePath(guid,
                            serviceSettings.TranscodeDirectory, videosize);

                        await StaticFunctions.TranscodeVideo(ffmpegEngine, default, strFilePath, strTranscodeFilePath,
                            videosize, originalVideoWidth, originalVideoHeight);

                        transcodeFileInfo = new FileInfo(strTranscodeFilePath);

                        if (!transcodeFileInfo.Exists)
                        {
                            return BadRequest("Transcoding failed, file does not exist.");
                        }
                        else if (transcodeFileInfo.Length == 0)
                        {
                            transcodeFileInfo.Delete();
                            return BadRequest(string.Format("Transcoding failed on file \"{0}\", file size is zero. Unfinished transcoded file \"{1}\" deleted.",
                                strFilePath, strTranscodeFilePath));
                        }

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
                    if (transcodeFileInfo != null && transcodeFileInfo.Exists)
                        transcodeFileInfo.Delete();

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
