namespace IDBrowserServiceCore.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("idUser")]
    public partial class idUser
    {
        [Key]
        [StringLength(50)]
        public string GUID { get; set; }

        //[StringLength(50)]
        //public string idUserName { get; set; }

        //[StringLength(50)]
        //public string idPassword { get; set; }

        //[StringLength(255)]
        //public string FullUserName { get; set; }

        //[StringLength(255)]
        //public string DefaultShare { get; set; }

        //public int? CatalogOwner { get; set; }

        //public int? UserOwner { get; set; }

        //public int? ImageOwner { get; set; }

        //[StringLength(100)]
        //public string EMailAddress { get; set; }

        //public DateTime? LastInlog { get; set; }

        //[StringLength(255)]
        //public string UDF1 { get; set; }

        //[StringLength(255)]
        //public string UDF2 { get; set; }

        //[StringLength(255)]
        //public string UDF3 { get; set; }

        //[StringLength(255)]
        //public string UDF4 { get; set; }

        //[StringLength(255)]
        //public string UDF5 { get; set; }

        //public DateTime? FirstInlog { get; set; }

        //public DateTime? PrevInlog { get; set; }

        //[StringLength(50)]
        //public string LastIP { get; set; }

        //public DateTime? idCreated { get; set; }

        //[StringLength(255)]
        //public string WebAddress { get; set; }

        //public long? idQuota { get; set; }

        //public DateTime? idQuotaExpires { get; set; }

        //public long? idDiskQuota { get; set; }

        //public DateTime? idDiskQuotaExpires { get; set; }

        //public long? idDiskUsage { get; set; }

        //[StringLength(10)]
        //public string idUserType { get; set; }

        //public int? FailedLoginCount { get; set; }

        //public DateTime? LoginLockExpires { get; set; }

        //public DateTime? LastFailedLogin { get; set; }

        //public int? TimezoneOffset { get; set; }

        //public int? idProtected { get; set; }

        //public int? idPublished { get; set; }

        //[Column(TypeName = "image")]
        //public byte[] CatalogCustom { get; set; }

        //[Column(TypeName = "image")]
        //public byte[] UserCustom { get; set; }

        //[Column(TypeName = "image")]
        //public byte[] ImageCustom { get; set; }
    }
}
