namespace IDBrowserServiceCore.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("idImageData")]
    public partial class idImageData
    {
        [Key]
        [StringLength(50)]
        public string GUID { get; set; }

        [StringLength(50)]
        public string ImageGUID { get; set; }

        [StringLength(50)]
        public string DataName { get; set; }

        //public DateTime? idCreated { get; set; }

        //public DateTime? idUpdated { get; set; }

        //[StringLength(50)]
        //public string Extension { get; set; }

        [Column(TypeName = "image")]
        public byte[] idData { get; set; }

        //public int? idMarked { get; set; }

        //[StringLength(20)]
        //public string idHashKey { get; set; }

        //[StringLength(50)]
        //public string TagString { get; set; }
    }
}
