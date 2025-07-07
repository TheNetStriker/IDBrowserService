namespace IDBrowserServiceCore.Data.IDImager
{
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

        [StringLength(500)]
        public string ContentType { get; set; }

        [StringLength(500)]
        public string ContentGroup { get; set; }

        public string ContentValue { get; set; }
    }
}
