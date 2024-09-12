using System;
using System.Collections.Generic;

namespace IDBrowserServiceCore.Settings
{
    public class ServiceSettings
    {
        public bool CreateThumbnails { get; set; }
        public int MThumbmailWidth { get; set; }
        public int MThumbnailHeight { get; set; }
        public List<FilePathReplaceSettings> FilePathReplace { get; set; }
        public string TranscodeDirectory { get; set; }
        public string TokenSecretKey { get; set; }
        public string TokenIssuer { get; set; }
        public string TokenAudience { get; set; }
        public TimeSpan TokenExpiration { get; set; }
        public bool DisableInsecureMediaPlayApi { get; set; }
        public bool EnableDatabaseCache { get; set; }
        public CronJobs CronJobs { get; set; }
        public string OpenIdConfigurationAddress { get; set; }
        public bool OpenIdDisableServerCertificateValidation { get; set; }
        public string OpenIdAudience { get; set; }

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
