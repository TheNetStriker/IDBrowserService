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

        public ServiceSettings()
        {
            FilePathReplace = new List<FilePathReplaceSettings>();

            // Default settings
            CreateThumbnails = true;
            MThumbmailWidth = 1680;
            MThumbnailHeight = 1260;
        }
    }
}
