using System;
using System.Collections.Generic;

namespace IDBrowserServiceCore.Settings
{
    public class ServiceSettings
    {
        public bool CreateThumbnails { get; set; }
        public uint MThumbmailWidth { get; set; }
        public uint MThumbnailHeight { get; set; }
        public List<FilePathReplaceSettings> FilePathReplace { get; set; }
        public string TranscodeDirectory { get; set; }
        public string TokenSecretKey { get; set; }
        public string TokenIssuer { get; set; }
        public string TokenAudience { get; set; }
        public TimeSpan TokenExpiration { get; set; }
        public bool DisableInsecureMediaPlayApi { get; set; }
        public bool EnableDatabaseCache { get; set; }
        public bool TranscodeInBackground { get; set; }
        /// <summary>
        /// Optional ffmpeg hardware acceleration method for transcoding.
        /// Valid values: None, cuda, cuvid, dxva2, qsv, d3d11va.
        /// Leave null or empty to use software encoding (default).
        /// Run 'ffmpeg -hwaccels' on the server to see what is available.
        /// </summary>
        public string TranscodeHWAccel { get; set; }
        public CronJobs CronJobs { get; set; }

        public ServiceSettings()
        {
            FilePathReplace = new List<FilePathReplaceSettings>();

            // Default settings
            CreateThumbnails = true;
            MThumbmailWidth = 1680;
            MThumbnailHeight = 1260;

            TokenSecretKey = "ThisIsUnsecurePleaseChangeMeAsSoonAsPossible";
            TokenIssuer = "IDBrowserServiceCore";
            TokenAudience = "IDBrowserUser";
            TokenExpiration = new TimeSpan(1, 0, 0);
            CronJobs = new CronJobs();
        }
    }
}
