using System.Text.Json.Serialization;

namespace IDBrowserServiceCore.Settings
{
    public class SiteSettings
    {
        // Will be set in Program.cs
        [JsonIgnore]
        public string SiteName { get; set; }
        public ConnectionStringSettings ConnectionStrings { get; set; }
        public ServiceSettings ServiceSettings { get; set; }
    }
}
