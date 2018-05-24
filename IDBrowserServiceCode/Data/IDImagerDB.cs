namespace IDBrowserServiceCode.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class IDImagerDB : DbContext
    {
        public IDImagerDB()
            : base("name=IDImagerEntities")
        {
        }

        public virtual DbSet<idCatalogItem> idCatalogItem { get; set; }
        public virtual DbSet<idCatalogItemDefinition> idCatalogItemDefinition { get; set; }
        public virtual DbSet<idFilePath> idFilePath { get; set; }
        public virtual DbSet<idImageData> idImageData { get; set; }
        public virtual DbSet<idImageVersion> idImageVersion { get; set; }
        public virtual DbSet<idProp> idProp { get; set; }
        public virtual DbSet<idPropCategory> idPropCategory { get; set; }
        public virtual DbSet<idSearchData> idSearchData { get; set; }
        public virtual DbSet<idUser> idUser { get; set; }
        public virtual DbSet<v_prop> v_prop { get; set; }
        public virtual DbSet<v_PropCategory> v_PropCategory { get; set; }
        public virtual DbSet<idThumbs> idThumbs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<idCatalogItem>()
                .HasMany(e => e.idCatalogItemDefinition)
                .WithRequired(e => e.idCatalogItem)
                .HasForeignKey(e => e.CatalogItemGUID);

            //modelBuilder.Entity<idCatalogItem>()
            //    .HasMany(e => e.idImageVersion)
            //    .WithOptional(e => e.idCatalogItem)
            //    .HasForeignKey(e => e.MainImageGUID)
            //    .WillCascadeOnDelete();

            modelBuilder.Entity<idFilePath>()
                .HasMany(e => e.idCatalogItem)
                .WithOptional(e => e.idFilePath)
                .HasForeignKey(e => e.PathGUID)
                .WillCascadeOnDelete();

            //modelBuilder.Entity<idFilePath>()
            //    .HasMany(e => e.idImageVersion)
            //    .WithOptional(e => e.idFilePath)
            //    .HasForeignKey(e => e.PathGUID);

            //modelBuilder.Entity<idUser>()
            //    .HasMany(e => e.idCatalogItem)
            //    .WithOptional(e => e.idUser)
            //    .HasForeignKey(e => e.UserGUID)
            //    .WillCascadeOnDelete();

            //modelBuilder.Entity<idUser>()
            //    .HasMany(e => e.idProp)
            //    .WithOptional(e => e.idUser)
            //    .HasForeignKey(e => e.UserGUID);
        }
    }
}
