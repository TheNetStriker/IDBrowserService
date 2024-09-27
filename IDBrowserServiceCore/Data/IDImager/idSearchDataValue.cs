namespace IDBrowserServiceCore.Data.IDImager
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("idSearchDataValue")]
    public partial class idSearchDataValue
    {
        [Key]
        [StringLength(50)]
        public string GUID { get; set; }

        [StringLength(4000)]
        public string ContentValue { get; set; }
    }
}
