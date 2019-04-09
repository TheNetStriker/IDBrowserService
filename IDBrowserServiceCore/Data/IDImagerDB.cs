namespace IDBrowserServiceCore.Data
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    public partial class IDImagerDB : DbContext
    {
        private string connectionString;

        public IDImagerDB(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<idCatalogItemDefinition>()
                .HasKey(c => new { c.GUID });
            //modelBuilder.Entity<idCatalogItem>()
            //    .HasMany(e => e.idCatalogItemDefinition)
            //    //.WithRequired(e => e.idCatalogItem)
            //    .HasForeignKey(e => e.CatalogItemGUID);

            //modelBuilder.Entity<idFilePath>()
            //    .HasMany(e => e.idCatalogItem)
            //    //.WithOptional(e => e.idFilePath)
            //    .HasForeignKey(e => e.PathGUID)
            //    .WillCascadeOnDelete();
        }
    }
}
