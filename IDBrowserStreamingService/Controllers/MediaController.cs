using Common.Logging;
using IDBrowserServiceCode;
using IDBrowserServiceCode.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Web.Configuration;
using System.Web.Http;

namespace IDBrowserStreamingService.Controllers
{
    public class MediaController : ApiController
    {
        #region Fields

        // This will be used in copying input stream to output stream.
        public const int ReadStreamBufferSize = 1024 * 1024;
        // We have a read-only dictionary for mapping file extensions and MIME names. 
        public static readonly IReadOnlyDictionary<string, string> MimeNames;

        private static ILog log;

        #endregion

        #region Constructors

        static MediaController()
        {
            if (log == null)
                log = log = LogManager.GetLogger(typeof(MediaController));

            var mimeNames = new Dictionary<string, string>();

            mimeNames.Add(".mp3", "audio/mpeg");    // List all supported media types; 
            mimeNames.Add(".mp4", "video/mp4");
            mimeNames.Add(".ogg", "application/ogg");
            mimeNames.Add(".ogv", "video/ogg");
            mimeNames.Add(".oga", "audio/ogg");
            mimeNames.Add(".wav", "audio/x-wav");
            mimeNames.Add(".webm", "video/webm");

            MimeNames = new ReadOnlyDictionary<string, string>(mimeNames);
        }
        #endregion

        #region Actions

        [HttpGet]
        public HttpResponseMessage Play(string guid)
        {
            try
            {
                // This can prevent some unnecessary accesses. 
                // These kind of file names won't be existing at all.
                if (string.IsNullOrWhiteSpace(guid) || guid.Contains("."))
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                String strFilePath = GetCatalogItemFilePath(guid);
                FileInfo fileInfo = new FileInfo(strFilePath);

                if (!fileInfo.Exists)
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                long totalLength = fileInfo.Length;

                RangeHeaderValue rangeHeader = base.Request.Headers.Range;
                HttpResponseMessage response = new HttpResponseMessage();

                response.Headers.AcceptRanges.Add("bytes");

                // The request will be treated as normal request if there is no Range header.
                if (rangeHeader == null || !rangeHeader.Ranges.Any())
                {
                    response.StatusCode = HttpStatusCode.OK;
                    response.Content = new PushStreamContent((outputStream, httpContent, transpContext)
                    =>
                    {
                        using (outputStream) // Copy the file to output stream straightforward. 
                        using (Stream inputStream = fileInfo.OpenRead())
                        {
                            try
                            {
                                inputStream.CopyTo(outputStream, ReadStreamBufferSize);
                            }
                            catch (Exception error)
                            {
                                Debug.WriteLine(error);
                            }
                        }
                    }, GetMimeNameFromExt(fileInfo.Extension));

                    response.Content.Headers.ContentLength = totalLength;
                    return response;
                }

                long start = 0, end = 0;

                // 1. If the unit is not 'bytes'.
                // 2. If there are multiple ranges in header value.
                // 3. If start or end position is greater than file length.
                if (rangeHeader.Unit != "bytes" || rangeHeader.Ranges.Count > 1 ||
                    !TryReadRangeItem(rangeHeader.Ranges.First(), totalLength, out start, out end))
                {
                    response.StatusCode = HttpStatusCode.RequestedRangeNotSatisfiable;
                    response.Content = new StreamContent(Stream.Null);  // No content for this status.
                    response.Content.Headers.ContentRange = new ContentRangeHeaderValue(totalLength);
                    response.Content.Headers.ContentType = GetMimeNameFromExt(fileInfo.Extension);

                    return response;
                }

                var contentRange = new ContentRangeHeaderValue(start, end, totalLength);

                // We are now ready to produce partial content.
                response.StatusCode = HttpStatusCode.PartialContent;
                response.Content = new PushStreamContent((outputStream, httpContent, transpContext)
                =>
                {
                    using (outputStream) // Copy the file to output stream in indicated range.
                    using (Stream inputStream = fileInfo.OpenRead())
                        CreatePartialContent(inputStream, outputStream, start, end);

                }, GetMimeNameFromExt(fileInfo.Extension));

                response.Content.Headers.ContentLength = end - start + 1;
                response.Content.Headers.ContentRange = contentRange;

                return response;
            }
            catch (Exception e)
            {
                log.Error(e);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.ToString());
            }
        }

        #endregion

        #region Others

        private static String GetCatalogItemFilePath(String guid)
        {
            IDImagerEntities db = new IDImagerEntities();
            idCatalogItem catalogItem = db.idCatalogItem.Include("idFilePath").Single(x => x.GUID.Equals(guid));
            db.Dispose();
            return StaticFunctions.GetImageFilePath(catalogItem);
        }

        private static MediaTypeHeaderValue GetMimeNameFromExt(string ext)
        {
            string value;

            if (MimeNames.TryGetValue(ext.ToLowerInvariant(), out value))
                return new MediaTypeHeaderValue(value);
            else
                return new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);
        }

        private static bool TryReadRangeItem(RangeItemHeaderValue range, long contentLength,
            out long start, out long end)
        {
            if (range.From != null)
            {
                start = range.From.Value;
                if (range.To != null)
                    end = range.To.Value;
                else
                    end = contentLength - 1;
            }
            else
            {
                end = contentLength - 1;
                if (range.To != null)
                    start = contentLength - range.To.Value;
                else
                    start = 0;
            }
            return (start < contentLength && end < contentLength);
        }

        private static void CreatePartialContent(Stream inputStream, Stream outputStream,
            long start, long end)
        {
            int count = 0;
            long remainingBytes = end - start + 1;
            long position = start;
            byte[] buffer = new byte[ReadStreamBufferSize];
            
            inputStream.Position = start;
            do
            {
                try
                {
                    if (remainingBytes > ReadStreamBufferSize)
                        count = inputStream.Read(buffer, 0, ReadStreamBufferSize);
                    else
                        count = inputStream.Read(buffer, 0, (int)remainingBytes); 
                    outputStream.Write(buffer, 0, count);
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                    break;
                }
                position = inputStream.Position;
                remainingBytes = end - position + 1;
            } while (position <= end);
        }

        #endregion
    }
}