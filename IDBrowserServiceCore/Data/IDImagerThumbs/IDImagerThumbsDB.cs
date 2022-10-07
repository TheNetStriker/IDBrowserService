namespace IDBrowserServiceCore.Data.IDImagerThumbs
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    public partial class IDImagerThumbsDB : DbContext
    {
        public IDImagerThumbsDB(DbContextOptions<IDImagerThumbsDB> options)
            : base(options)
        {
            if (Database.ProviderName.Equals("Npgsql.EntityFrameworkCore.PostgreSQL"))
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            }
        }

        public virtual DbSet<idThumbs> idThumbs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
