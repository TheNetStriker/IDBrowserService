//------------------------------------------------------------------------------
// <auto-generated>
//     Der Code wurde von einer Vorlage generiert.
//
//     Manuelle Änderungen an dieser Datei führen möglicherweise zu unerwartetem Verhalten der Anwendung.
//     Manuelle Änderungen an dieser Datei werden überschrieben, wenn der Code neu generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IDBrowserServiceCode.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class idProp
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public idProp()
        {
            this.idCatalogItemDefinition = new HashSet<idCatalogItemDefinition>();
            this.idPropRelation = new HashSet<idPropRelation>();
            this.idUserProp = new HashSet<idUserProp>();
        }
    
        public string GUID { get; set; }
        public string ParentGUID { get; set; }
        public string PropName { get; set; }
        public string PropValue { get; set; }
        public Nullable<int> Quick { get; set; }
        public string UserGUID { get; set; }
        public Nullable<System.DateTime> idCreated { get; set; }
        public Nullable<System.DateTime> idLastAccess { get; set; }
        public string PropXMPLink { get; set; }
        public Nullable<int> lft { get; set; }
        public Nullable<int> rgt { get; set; }
        public byte[] idImage { get; set; }
        public Nullable<int> ParentAssign { get; set; }
        public Nullable<int> ParentXMPLinkAssign { get; set; }
        public string idSynonyms { get; set; }
        public Nullable<double> idGPSLon { get; set; }
        public Nullable<double> idGPSLat { get; set; }
        public Nullable<double> idGPSAlt { get; set; }
        public Nullable<int> idGPSGeoTag { get; set; }
        public Nullable<int> idGPSGeoTagIfExist { get; set; }
        public Nullable<double> idGPSRadius { get; set; }
        public Nullable<int> idShortCut { get; set; }
        public Nullable<int> MutualExclusive { get; set; }
        public string idDescription { get; set; }
        public byte[] idDetails { get; set; }
        public byte[] idProps { get; set; }
        public Nullable<int> ApplyProps { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idCatalogItemDefinition> idCatalogItemDefinition { get; set; }
        public virtual idUser idUser { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idPropRelation> idPropRelation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idUserProp> idUserProp { get; set; }
    }
}