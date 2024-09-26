using System.Collections.Generic;

namespace IDBrowserServiceCore.Settings
{
    public class IDBrowserConfiguration
    {
        public IDBrowserConfiguration()
        {
            ImageFileExtensions = new List<string>();
            VideoFileExtensions = new List<string>();
            Sites = new Dictionary<string, SiteSettings>();

            // Default values
            UseResponseCompression = true;
            UseSwagger = true;
        }

        public List<string> ImageFileExtensions { get; set; }
        public List<string> VideoFileExtensions { get; set; }
        public bool UseResponseCompression { get; set; }
        public bool UseSwagger { get; set; }
        public Dictionary<string, SiteSettings> Sites { get; set; }
    }
}
