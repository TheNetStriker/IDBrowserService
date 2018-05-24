namespace IDBrowserServiceCore.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("idCatalogItemDefinition")]
    public partial class idCatalogItemDefinition
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string GUID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string CatalogItemGUID { get; set; }

        public DateTime? idAssigned { get; set; }

        public virtual idCatalogItem idCatalogItem { get; set; }

        //public virtual idProp idProp { get; set; }
    }
}
