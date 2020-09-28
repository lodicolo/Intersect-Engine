using JetBrains.Annotations;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Intersect.Server.Prototype
{
    public class PrototypeContext : DbContext
    {
        public DbSet<PrototypeSetEntity> Sets { get; set; }

        public DbSet<PrototypeJunctionEntity> Junctions { get; set; }

        public DbSet<PrototypeSimpleEntity> Simples { get; set; }

        public DbSet<ContentString> ContentStrings { get; set; }

        public PrototypeContext()
        {
        }

        protected override void OnConfiguring([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var csb = new SqliteConnectionStringBuilder(@"Data Source=prototype.db");
            var connectionString = csb.ToString();

            optionsBuilder.UseSqlite(connectionString);
        }

        protected override void OnModelCreating([NotNull] ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PrototypeJunctionEntity>()
                .HasOne(e => e.Set)
                .WithMany(s => s.Junctions)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrototypeJunctionEntity>()
                .HasOne(e => e.Simple)
                .WithMany(s => s.Junctions)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrototypeJunctionEntity>().HasIndex(e => new {e.SetId, e.SimpleId}).IsUnique(true);

            ContentString.OnModelCreating(modelBuilder);
        }
    }
}
