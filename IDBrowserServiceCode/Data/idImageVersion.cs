namespace IDBrowserServiceCode.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("idImageVersion")]
    public partial class idImageVersion
    {
        [Key]
        [StringLength(50)]
        public string GUID { get; set; }

        [StringLength(50)]
        public string MainImageGUID { get; set; }

        //public DateTime? DateCreated { get; set; }

        //[StringLength(1024)]
        //public string VersionName { get; set; }

        //[StringLength(1024)]
        //public string FileName { get; set; }

        //public int? FileSize { get; set; }

        //public DateTime? FileStamp { get; set; }

        //public int? Rating { get; set; }

        //[StringLength(50)]
        //public string idLabel { get; set; }

        public int? idInSync { get; set; }

        //public int? idBookmark { get; set; }

        //public int? idFileNameLen { get; set; }

        //[StringLength(50)]
        //public string PathGUID { get; set; }

        //[StringLength(50)]
        //public string idFileType { get; set; }

        //[StringLength(50)]
        //public string GroupGUID { get; set; }

        //public int? GroupOrder { get; set; }

        //public double? idGPSLon { get; set; }

        //public double? idGPSLat { get; set; }

        //public double? idGPSAlt { get; set; }

        //public int? idHasRecipe { get; set; }

        //[StringLength(50)]
        //public string idSignature { get; set; }

        //public DateTime? idCreated { get; set; }

        //public DateTime? LastUpdate { get; set; }

        //[StringLength(50)]
        //public string idSimilarityCode { get; set; }

        //public int? idYear { get; set; }

        //public int? idMonth { get; set; }

        //public int? idDay { get; set; }

        //public int? idWeek { get; set; }

        //public DateTime? DateTimeStamp { get; set; }

        //public int? idWeekDay { get; set; }

        //public virtual idCatalogItem idCatalogItem { get; set; }
    }
}
