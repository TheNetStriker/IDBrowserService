namespace IDBrowserServiceCode.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("idPropCategory")]
    public partial class idPropCategory
    {
        [Key]
        [StringLength(50)]
        public string GUID { get; set; }

        [StringLength(255)]
        public string CategoryName { get; set; }

        //public int? IsPrivate { get; set; }

        //public double? idUseColor { get; set; }

        //public double? idColor { get; set; }

        //public double? idFontColor { get; set; }

        //public int? idOrder { get; set; }

        [Column(TypeName = "image")]
        public byte[] idImage { get; set; }
    }
}
