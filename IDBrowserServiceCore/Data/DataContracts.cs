using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IDBrowserServiceCore.Data
{
    [DataContract]
    public class ImageProperty
    {
        [DataMember]
        public string GUID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int? ImageCount { get; set; }

        [DataMember]
        public int SubPropertyCount { get; set; }
    }

    [DataContract]
    public class ImagePropertyRecursive
    {
        [DataMember]
        public string GUID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string FullRecursivePath { get; set; }
    }

    [DataContract]
    public class CatalogItem
    {
        [DataMember]
        public string GUID { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string FileType { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public int? HasRecipe { get; set; }
    }

    [DataContract]
    public class ImageInfo
    {
        [DataMember]
        public String ImageName { get; set; }

        [DataMember]
        public String ImageDescription { get; set; }

        [DataMember]
        public String ImageResolution { get; set; }

        [DataMember]
        public String FileType { get; set; }

        [DataMember]
        public DateTime? Timestamp { get; set; }

        [DataMember]
        public int? FileSize { get; set; }

        [DataMember]
        public int? Rating { get; set; }

        [DataMember]
        public Nullable<double> GPSLat { get; set; }

        [DataMember]
        public Nullable<double> GPSLon { get; set; }

        [DataMember]
        public List<XmpProperty> XmpProperties { get; set; }
    }

    [DataContract]
    public class XmpProperty
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }
    }

    [DataContract]
    public class FilePath
    {
        [DataMember]
        public string GUID { get; set; }

        [DataMember]
        public string MediumName { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string RootName { get; set; }

        [DataMember]
        public int ImageCount { get; set; }
    }
}
