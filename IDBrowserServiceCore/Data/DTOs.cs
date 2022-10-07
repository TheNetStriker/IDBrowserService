using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace IDBrowserServiceCore.Data
{
    public class ImageProperty
    {
        public string GUID { get; set; }

        [JsonIgnore]
        public string ParentGUID { get; set; }

        public string Name { get; set; }

        public int? ImageCount { get; set; }

        public int SubPropertyCount { get; set; }
    }

    public class ImagePropertyRecursive
    {
        public string GUID { get; set; }

        public string Name { get; set; }

        public string FullRecursivePath { get; set; }
    }

    public class CatalogItem
    {
        public string GUID { get; set; }

        public string FileName { get; set; }

        public string FileType { get; set; }

        public string FilePath { get; set; }

        public int? HasRecipe { get; set; }
    }

    public class ImageInfo
    {
        //This is now included in idsearchdata
        //public String ImageName { get; set; }

        //This is now included in idsearchdata
        //public String ImageDescription { get; set; }

        public String ImageResolution { get; set; }

        public String FileType { get; set; }

        public DateTime? Timestamp { get; set; }

        public int? FileSize { get; set; }

        public int? Rating { get; set; }

        public Nullable<double> GPSLat { get; set; }

        public Nullable<double> GPSLon { get; set; }

        public List<XmpProperty> XmpProperties { get; set; }
    }

    public class XmpProperty
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
    public class FilePath
    {
        public string GUID { get; set; }

        public string MediumName { get; set; }

        public string Path { get; set; }

        public string RootName { get; set; }

        public int ImageCount { get; set; }
    }

    public class MediaToken
    {
        public string Token { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }
    }
}
