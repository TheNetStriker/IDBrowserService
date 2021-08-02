namespace IDBrowserServiceCore.Data.IDImager
{
    using Microsoft.EntityFrameworkCore;

    public partial class IDImagerDB : DbContext
    {
        public IDImagerDB(DbContextOptions<IDImagerDB> options)
            : base(options)
        { }

        public virtual DbSet<idCatalogItem> idCatalogItem { get; set; }
        public virtual DbSet<idCatalogItemDefinition> idCatalogItemDefinition { get; set; }
        public virtual DbSet<idFilePath> idFilePath { get; set; }
        public virtual DbSet<idCache_FilePath> idCache_FilePath { get; set; }
        public virtual DbSet<idMediumInfo> idMediumInfo { get; set; }
        public virtual DbSet<idImageData> idImageData { get; set; }
        public virtual DbSet<idImageVersion> idImageVersion { get; set; }
        public virtual DbSet<idProp> idProp { get; set; }
        public virtual DbSet<idPropCategory> idPropCategory { get; set; }
        public virtual DbSet<idSearchData> idSearchData { get; set; }
        public virtual DbSet<idUser> idUser { get; set; }
        public virtual DbSet<v_prop> v_prop { get; set; }
        public virtual DbSet<v_PropCategory> v_PropCategory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<idCatalogItemDefinition>()
                .HasKey(table => new {
                    table.GUID,
                    table.CatalogItemGUID
            });

            modelBuilder.Entity<idCatalogItem>()
                .HasMany(e => e.idCatalogItemDefinition)
                .WithOne(e => e.idCatalogItem)
                .HasForeignKey(e => e.CatalogItemGUID);

            modelBuilder.Entity<idFilePath>()
                .HasMany(e => e.idCatalogItem)
                .WithOne(e => e.idFilePath)
                .HasForeignKey(e => e.PathGUID);

            modelBuilder.Entity<idMediumInfo>()
                .HasMany(e => e.idFilePath)
                .WithOne(e => e.idMediumInfo)
                .HasForeignKey(e => e.MediumGUID);

            modelBuilder.Entity<idCache_FilePath>()
                .HasMany(e => e.idCatalogItem)
                .WithOne(e => e.idCache_FilePath)
                .HasForeignKey(e => e.PathGUID);

            modelBuilder.Entity<idFilePath>()
                .HasOne(e => e.idCache_FilePath)
                .WithOne(e => e.idFilePath)
                .HasForeignKey<idFilePath>(e => e.guid);

            modelBuilder.Entity<idCache_FilePath>()
                .HasOne(e => e.root_idCache_FilePath)
                .WithMany()
                .HasForeignKey(e => e.RootGUID);

            modelBuilder.Entity<idProp>()
                .HasMany(e => e.idCatalogItemDefinition)
                .WithOne(e => e.idProp)
                .HasForeignKey(e => e.GUID);
        }
    }
}
