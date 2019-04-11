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
        { }

        public virtual DbSet<idThumbs> idThumbs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
