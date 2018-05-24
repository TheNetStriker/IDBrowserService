namespace IDBrowserServiceCode.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("idCatalogItem")]
    public partial class idCatalogItem
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public idCatalogItem()
        {
            idCatalogItemDefinition = new HashSet<idCatalogItemDefinition>();
            //idImageVersion = new HashSet<idImageVersion>();
        }

        [Key]
        [StringLength(50)]
        public string GUID { get; set; }

        [StringLength(1024)]
        public string FileName { get; set; }

        public DateTime? DateTimeStamp { get; set; }

        public int? FileSize { get; set; }

        //public DateTime? FileStamp { get; set; }

        public int? Rating { get; set; }

        [StringLength(1024)]
        public string ImageName { get; set; }

        [Column(TypeName = "ntext")]
        public string ImageDescription { get; set; }

        //[StringLength(255)]
        //public string UDF1 { get; set; }

        [StringLength(255)]
        public string UDF2 { get; set; }

        //[StringLength(255)]
        //public string UDF3 { get; set; }

        //[StringLength(255)]
        //public string UDF4 { get; set; }

        //[StringLength(255)]
        //public string UDF5 { get; set; }

        //[StringLength(50)]
        //public string UserGUID { get; set; }

        //public int? AutoCataloged { get; set; }

        //public DateTime? LastUpdate { get; set; }

        public DateTime? idCreated { get; set; }

        //public int? idCounter { get; set; }

        //public int? idMarked { get; set; }

        //[StringLength(50)]
        //public string idLabel { get; set; }

        public int? idInSync { get; set; }

        //public int? idBookmark { get; set; }

        //public int? idFileNameLen { get; set; }

        [StringLength(50)]
        public string PathGUID { get; set; }

        [StringLength(50)]
        public string idFileType { get; set; }

        //[StringLength(50)]
        //public string GroupGUID { get; set; }

        //public int? GroupOrder { get; set; }

        public double? idGPSLon { get; set; }

        public double? idGPSLat { get; set; }

        //public double? idGPSAlt { get; set; }

        public int? idHasRecipe { get; set; }

        //[StringLength(50)]
        //public string idSignature { get; set; }

        //[StringLength(50)]
        //public string idSimilarityCode { get; set; }

        //public int? idYear { get; set; }

        //public int? idMonth { get; set; }

        //public int? idDay { get; set; }

        //public int? idWeek { get; set; }

        //public int? idWeekDay { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idCatalogItemDefinition> idCatalogItemDefinition { get; set; }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<idImageVersion> idImageVersion { get; set; }

        public virtual idFilePath idFilePath { get; set; }
    }
}
