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
    
    public partial class idCatalogItem
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public idCatalogItem()
        {
            this.idCatalogItemDefinition = new HashSet<idCatalogItemDefinition>();
            this.idImageComment = new HashSet<idImageComment>();
            this.idUserFavorite = new HashSet<idUserFavorite>();
            this.idImageCard = new HashSet<idImageCard>();
            this.idImageCollectionItem = new HashSet<idImageCollectionItem>();
            this.idImageIPTC = new HashSet<idImageIPTC>();
            this.idImageSubscription = new HashSet<idImageSubscription>();
            this.idImageVersion = new HashSet<idImageVersion>();
        }
    
        public string GUID { get; set; }
        public string FileName { get; set; }
        public Nullable<System.DateTime> DateTimeStamp { get; set; }
        public Nullable<int> FileSize { get; set; }
        public Nullable<System.DateTime> FileStamp { get; set; }
        public Nullable<int> Rating { get; set; }
        public string ImageName { get; set; }
        public string ImageDescription { get; set; }
        public string UDF1 { get; set; }
        public string UDF2 { get; set; }
        public string UDF3 { get; set; }
        public string UDF4 { get; set; }
        public string UDF5 { get; set; }
        public string UserGUID { get; set; }
        public Nullable<int> AutoCataloged { get; set; }
        public Nullable<System.DateTime> LastUpdate { get; set; }
        public Nullable<System.DateTime> idCreated { get; set; }
        public Nullable<int> idCounter { get; set; }
        public Nullable<int> idMarked { get; set; }
        public string idLabel { get; set; }
        public Nullable<int> idInSync { get; set; }
        public Nullable<int> idBookmark { get; set; }
        public Nullable<int> idFileNameLen { get; set; }
        public string PathGUID { get; set; }
        public string idFileType { get; set; }
        public string GroupGUID { get; set; }
        public Nullable<int> GroupOrder { get; set; }
        public Nullable<double> idGPSLon { get; set; }
        public Nullable<double> idGPSLat { get; set; }
        public Nullable<double> idGPSAlt { get; set; }
        public Nullable<int> idHasRecipe { get; set; }
        public string idSignature { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idCatalogItemDefinition> idCatalogItemDefinition { get; set; }
        public virtual idUser idUser { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idImageComment> idImageComment { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idUserFavorite> idUserFavorite { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idImageCard> idImageCard { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idImageCollectionItem> idImageCollectionItem { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idImageIPTC> idImageIPTC { get; set; }
        public virtual idFilePath idFilePath { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idImageSubscription> idImageSubscription { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idImageVersion> idImageVersion { get; set; }
    }
}