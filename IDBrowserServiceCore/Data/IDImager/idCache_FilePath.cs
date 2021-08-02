namespace IDBrowserServiceCore.Data.IDImager
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("idCache_FilePath")]
    public partial class idCache_FilePath
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public idCache_FilePath()
        {
            idCatalogItem = new HashSet<idCatalogItem>();
        }

        [Key]
        [StringLength(50)]
        public string FilePathGUID { get; set; }

        [StringLength(50)]
        public string GUID { get; set; }

        [StringLength(50)]
        public string RootGUID { get; set; }

        [StringLength(5000)]
        public string FilePath { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idCatalogItem> idCatalogItem { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual idCache_FilePath root_idCache_FilePath { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual idFilePath idFilePath { get; set; }
    }
}
