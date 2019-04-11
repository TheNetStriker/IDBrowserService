namespace IDBrowserServiceCore.Data.IDImager
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("idSearchData")]
    public partial class idSearchData
    {
        [Key]
        [StringLength(50)]
        public string GUID { get; set; }

        [StringLength(50)]
        public string RelatedGUID { get; set; }

        [StringLength(150)]
        public string ContentType { get; set; }

        [StringLength(150)]
        public string ContentGroup { get; set; }

        [StringLength(4000)]
        public string ContentValue { get; set; }
    }
}
