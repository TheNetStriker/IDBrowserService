namespace IDBrowserServiceCore.Data.IDImager
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("idMediumInfo")]
    public partial class idMediumInfo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public idMediumInfo()
        {
            idFilePath = new HashSet<idFilePath>();
        }

        [Key]
        [StringLength(50)]
        public string GUID { get; set; }

        [StringLength(255)]
        public string MediumName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idFilePath> idFilePath { get; set; }
    }
}
