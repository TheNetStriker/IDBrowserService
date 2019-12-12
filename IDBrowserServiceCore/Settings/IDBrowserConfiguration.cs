using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Settings
{
    public class IDBrowserConfiguration
    {
        public string Urls { get; set; }
        public List<string> ImageFileExtensions { get; set; }
        public List<string> VideoFileExtensions { get; set; }
        public bool UseResponseCompression { get; set; }
        public Dictionary<string, SiteSettings> Sites { get; set; }
    }
}
