namespace IDBrowserServiceCore.Data.IDImager
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("idCache_Prop")]
    public partial class idCache_Prop
    {
        [Key]
        [StringLength(50)]
        public string PropGuid { get; set; }

        [StringLength(50)]
        public string CategoryGuid { get; set; }
    }
}
