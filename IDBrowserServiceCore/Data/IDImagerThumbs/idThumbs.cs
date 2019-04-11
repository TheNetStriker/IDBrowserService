namespace IDBrowserServiceCore.Data.IDImagerThumbs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class idThumbs
    {
        [Key]
        [StringLength(50)]
        public string GUID { get; set; }

        [StringLength(255)]
        public string ImageGUID { get; set; }

        [StringLength(1)]
        public string idType { get; set; }

        //[StringLength(255)]
        //public string FileName { get; set; }

        //[StringLength(2)]
        //public string MediumType { get; set; }

        //[StringLength(128)]
        //public string MediumName { get; set; }

        //public float? MediumSerial { get; set; }

        [Column(TypeName = "image")]
        public byte[] idThumb { get; set; }
    }
}
