using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kbs.IdoWeb.Data.Information
{
    public partial class InformationContext : DbContext
    {
        public InformationContext()
        {
        }

        public InformationContext(DbContextOptions<InformationContext> options)
            : base(options)
        {
        }

        public virtual DbSet<RedListType> RedListType { get; set; }
        public virtual DbSet<Region> Region { get; set; }
        public virtual DbSet<RegionState> RegionState { get; set; }
        public virtual DbSet<Taxon> Taxon { get; set; }
        public virtual DbSet<TaxonToRegion> TaxonToRegion { get; set; }
        public virtual DbSet<TaxonomyState> TaxonomyState { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Name=DatabaseConnection");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<RedListType>(entity =>
            {
                entity.Property(e => e.RedListTypeId).ValueGeneratedNever();
            });

            modelBuilder.Entity<Region>(entity =>
            {
                entity.Property(e => e.RegionId).HasDefaultValueSql("nextval('\"Inf\".\"Region_RegionId_seq\"'::regclass)");

                entity.HasOne(d => d.SubRegionOf)
                    .WithMany(p => p.InverseSubRegionOf)
                    .HasForeignKey(d => d.SubRegionOfId)
                    .HasConstraintName("Region_SubRegionOfId_fkey");
            });

            modelBuilder.Entity<RegionState>(entity =>
            {
                entity.Property(e => e.RegionStateId).ValueGeneratedNever();
            });

            modelBuilder.Entity<Taxon>(entity =>
            {
                entity.Property(e => e.TaxonId).HasDefaultValueSql("nextval('\"Inf\".\"Taxon_TaxonId_seq\"'::regclass)");

                entity.Property(e => e.HasBracketDescription).HasDefaultValueSql("false");

                entity.Property(e => e.HasTaxDescChildren).HasDefaultValueSql("false");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.InverseClass)
                    .HasForeignKey(d => d.ClassId)
                    .HasConstraintName("Taxon_ClassId_fkey");

                entity.HasOne(d => d.Family)
                    .WithMany(p => p.InverseFamily)
                    .HasForeignKey(d => d.FamilyId)
                    .HasConstraintName("Taxon_FamilyId_fkey");

                entity.HasOne(d => d.Genus)
                    .WithMany(p => p.InverseGenus)
                    .HasForeignKey(d => d.GenusId)
                    .HasConstraintName("Taxon_GenusId_fkey");

                entity.HasOne(d => d.Kingdom)
                    .WithMany(p => p.InverseKingdom)
                    .HasForeignKey(d => d.KingdomId)
                    .HasConstraintName("Taxon_KingdomId_fkey");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.InverseOrder)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("Taxon_OrderId_fkey");

                entity.HasOne(d => d.Phylum)
                    .WithMany(p => p.InversePhylum)
                    .HasForeignKey(d => d.PhylumId)
                    .HasConstraintName("Taxon_PhylumId_fkey");

                entity.HasOne(d => d.RedListType)
                    .WithMany(p => p.Taxon)
                    .HasForeignKey(d => d.RedListTypeId)
                    .HasConstraintName("Taxon_RedListTypeId_fkey");

                entity.HasOne(d => d.Species)
                    .WithMany(p => p.InverseSpecies)
                    .HasForeignKey(d => d.SpeciesId)
                    .HasConstraintName("Taxon_SpeciesId_fkey");

                entity.HasOne(d => d.Subclass)
                    .WithMany(p => p.InverseSubclass)
                    .HasForeignKey(d => d.SubclassId)
                    .HasConstraintName("Taxon_SubclassId_fkey");

                entity.HasOne(d => d.Subfamily)
                    .WithMany(p => p.InverseSubfamily)
                    .HasForeignKey(d => d.SubfamilyId)
                    .HasConstraintName("Taxon_SubfamilyId_fkey");

                entity.HasOne(d => d.Suborder)
                    .WithMany(p => p.InverseSuborder)
                    .HasForeignKey(d => d.SuborderId)
                    .HasConstraintName("Taxon_SuborderId_fkey");

                entity.HasOne(d => d.Subphylum)
                    .WithMany(p => p.InverseSubphylum)
                    .HasForeignKey(d => d.SubphylumId)
                    .HasConstraintName("Taxon_SubphylumId_fkey");

                entity.HasOne(d => d.TaxonomyState)
                    .WithMany(p => p.Taxon)
                    .HasForeignKey(d => d.TaxonomyStateId)
                    .HasConstraintName("Taxon_TaxonomyStateId_fkey");
            });

            modelBuilder.Entity<TaxonToRegion>(entity =>
            {
                entity.HasKey(e => e.TaxonRegionId)
                    .HasName("TaxonToRegion_pkey");

                entity.Property(e => e.TaxonRegionId).HasDefaultValueSql("nextval('\"Inf\".\"TaxonToRegion_TaxonRegionId_seq\"'::regclass)");

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.TaxonToRegion)
                    .HasForeignKey(d => d.RegionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TaxonToRegion_RegionId_fkey");

                entity.HasOne(d => d.Taxon)
                    .WithMany(p => p.TaxonToRegion)
                    .HasForeignKey(d => d.TaxonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TaxonToRegion_TaxonId_fkey");
            });

            modelBuilder.Entity<TaxonomyState>(entity =>
            {
                entity.HasKey(e => e.StateId)
                    .HasName("TaxonomyState_pkey");

                entity.Property(e => e.StateId).ValueGeneratedNever();
            });

            modelBuilder.HasSequence<int>("Region_RegionId_seq");

            modelBuilder.HasSequence<int>("Taxon_TaxonId_seq");

            modelBuilder.HasSequence<int>("TaxonToRegion_TaxonRegionId_seq");
        }
    }
}
