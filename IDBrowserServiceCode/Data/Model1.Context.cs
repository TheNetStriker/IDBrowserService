﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class IDImagerEntities : DbContext
    {
        public IDImagerEntities()
            : base("name=IDImagerEntities")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<idCatalogItem> idCatalogItem { get; set; }
        public virtual DbSet<idCatalogItemDefinition> idCatalogItemDefinition { get; set; }
        public virtual DbSet<idCollectionInfo> idCollectionInfo { get; set; }
        public virtual DbSet<idFilePath> idFilePath { get; set; }
        public virtual DbSet<idImageArea> idImageArea { get; set; }
        public virtual DbSet<idImageBox> idImageBox { get; set; }
        public virtual DbSet<idImageCard> idImageCard { get; set; }
        public virtual DbSet<idImageCollection> idImageCollection { get; set; }
        public virtual DbSet<idImageCollectionItem> idImageCollectionItem { get; set; }
        public virtual DbSet<idImageComment> idImageComment { get; set; }
        public virtual DbSet<idImageData> idImageData { get; set; }
        public virtual DbSet<idImageGallery> idImageGallery { get; set; }
        public virtual DbSet<idImageIPTC> idImageIPTC { get; set; }
        public virtual DbSet<idImageModel> idImageModel { get; set; }
        public virtual DbSet<idImageScore> idImageScore { get; set; }
        public virtual DbSet<idImageSubscription> idImageSubscription { get; set; }
        public virtual DbSet<idImageVersion> idImageVersion { get; set; }
        public virtual DbSet<idPlaceHolder> idPlaceHolder { get; set; }
        public virtual DbSet<idProp> idProp { get; set; }
        public virtual DbSet<idPropCategory> idPropCategory { get; set; }
        public virtual DbSet<idPropGroup> idPropGroup { get; set; }
        public virtual DbSet<idPropRelation> idPropRelation { get; set; }
        public virtual DbSet<idSearchData> idSearchData { get; set; }
        public virtual DbSet<idSyncImageIPTC> idSyncImageIPTC { get; set; }
        public virtual DbSet<idSyncThumb> idSyncThumb { get; set; }
        public virtual DbSet<idSyncXMP> idSyncXMP { get; set; }
        public virtual DbSet<idThumbs> idThumbs { get; set; }
        public virtual DbSet<idUser> idUser { get; set; }
        public virtual DbSet<idUserFavorite> idUserFavorite { get; set; }
        public virtual DbSet<idUserProp> idUserProp { get; set; }
        public virtual DbSet<idUserUsage> idUserUsage { get; set; }
        public virtual DbSet<idUserUser> idUserUser { get; set; }
        public virtual DbSet<v_AlbumDisplayVersionPlaceHolder> v_AlbumDisplayVersionPlaceHolder { get; set; }
        public virtual DbSet<v_AlbumVersion> v_AlbumVersion { get; set; }
        public virtual DbSet<v_CatalogItem> v_CatalogItem { get; set; }
        public virtual DbSet<v_CatalogItemPeriods> v_CatalogItemPeriods { get; set; }
        public virtual DbSet<v_CatalogItemVersion> v_CatalogItemVersion { get; set; }
        public virtual DbSet<v_CatalogPeriods> v_CatalogPeriods { get; set; }
        public virtual DbSet<v_CatalogVersionPeriods> v_CatalogVersionPeriods { get; set; }
        public virtual DbSet<v_ImageVersion> v_ImageVersion { get; set; }
        public virtual DbSet<v_PlaceHolderVersion> v_PlaceHolderVersion { get; set; }
        public virtual DbSet<v_Prop> v_Prop { get; set; }
        public virtual DbSet<v_PropCategory> v_PropCategory { get; set; }
        public virtual DbSet<v_VersionPlaceHolder> v_VersionPlaceHolder { get; set; }
    }
}