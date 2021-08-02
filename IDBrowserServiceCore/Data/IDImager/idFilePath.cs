namespace IDBrowserServiceCore.Data.IDImager
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("idFilePath")]
    public partial class idFilePath
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public idFilePath()
        {
            idCatalogItem = new HashSet<idCatalogItem>();
        }

        [Key]
        [StringLength(50)]
        public string guid { get; set; }

        [StringLength(50)]
        public string MediumGUID { get; set; }

        //[StringLength(2)]
        //public string MediumType { get; set; }

        //[StringLength(255)]
        //public string MediumName { get; set; }

        //public double? MediumSerial { get; set; }

        //[StringLength(1024)]
        //public string FilePath { get; set; }

        //[StringLength(512)]
        //public string RootName { get; set; }

        //public int? PathState { get; set; }

        //public DateTime? idSignatureDate { get; set; }

        //public int? idIgnored { get; set; }

        //[StringLength(512)]
        //public string FolderName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idCatalogItem> idCatalogItem { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual idMediumInfo idMediumInfo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual idCache_FilePath idCache_FilePath { get; set; }
    }
}
