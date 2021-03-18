using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kbs.IdoWeb.Data.Mapping
{
    public partial class MappingContext : DbContext
    {
        public MappingContext()
        {
        }

        public MappingContext(DbContextOptions<MappingContext> options)
            : base(options)
        {
        }

        public virtual DbSet<OsmNewPlaces> OsmNewPlaces { get; set; }
        public virtual DbSet<OsmNewTk25> OsmNewTk25 { get; set; }
        public virtual DbSet<Tk25> Tk25 { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Name=DatabaseConnection");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("postgis")
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<OsmNewTk25>(entity =>
            {
                entity.HasKey(e => e.Gid)
                    .HasName("osm_new_tk25_pkey");
            });

            modelBuilder.Entity<Tk25>(entity =>
            {
                entity.Property(e => e.Tk25Id).ValueGeneratedNever();
            });

            modelBuilder.HasSequence("osm_new_places_id_seq").StartsAt(126258);

            modelBuilder.HasSequence("osm_new_tk25_gid_seq").StartsAt(2976);
        }
    }
}
