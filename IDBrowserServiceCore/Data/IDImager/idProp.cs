namespace IDBrowserServiceCore.Data.IDImager
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("idProp")]
    public partial class idProp
    {
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        //public idProp()
        //{
        //    idCatalogItemDefinition = new HashSet<idCatalogItemDefinition>();
        //}

        [Key]
        [StringLength(50)]
        public string GUID { get; set; }

        [StringLength(50)]
        public string ParentGUID { get; set; }

        [StringLength(255)]
        public string PropName { get; set; }

        [StringLength(255)]
        public string PropValue { get; set; }

        public int? Quick { get; set; }

        [StringLength(50)]
        public string UserGUID { get; set; }

        public DateTime? idCreated { get; set; }

        public DateTime? idLastAccess { get; set; }

        [StringLength(255)]
        public string PropXMPLink { get; set; }

        public int? lft { get; set; }

        public int? rgt { get; set; }

        [Column(TypeName = "image")]
        public byte[] idImage { get; set; }

        public int? ParentAssign { get; set; }

        public int? ParentXMPLinkAssign { get; set; }

        [StringLength(512)]
        public string idSynonyms { get; set; }

        public double? idGPSLon { get; set; }

        public double? idGPSLat { get; set; }

        public double? idGPSAlt { get; set; }

        public int? idGPSGeoTag { get; set; }

        public int? idGPSGeoTagIfExist { get; set; }

        public double? idGPSRadius { get; set; }

        public int? idShortCut { get; set; }

        public int? MutualExclusive { get; set; }

        [StringLength(1024)]
        public string idDescription { get; set; }

        [Column(TypeName = "image")]
        public byte[] idDetails { get; set; }

        [Column(TypeName = "image")]
        public byte[] idProps { get; set; }

        public int? ApplyProps { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idCatalogItemDefinition> idCatalogItemDefinition { get; set; }
    }
}
