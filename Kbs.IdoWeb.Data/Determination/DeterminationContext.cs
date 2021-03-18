using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kbs.IdoWeb.Data.Determination
{
    public partial class DeterminationContext : DbContext
    {
        public DeterminationContext()
        {
        }

        public DeterminationContext(DbContextOptions<DeterminationContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DescriptionKey> DescriptionKey { get; set; }
        public virtual DbSet<DescriptionKeyGroup> DescriptionKeyGroup { get; set; }
        public virtual DbSet<DescriptionKeyType> DescriptionKeyType { get; set; }
        public virtual DbSet<DescriptionStep> DescriptionStep { get; set; }
        public virtual DbSet<TaxonDescription> TaxonDescription { get; set; }
        public virtual DbSet<VisibilityCategory> VisibilityCategory { get; set; }

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

            modelBuilder.Entity<DescriptionKey>(entity =>
            {
                entity.Property(e => e.DescriptionKeyId).HasDefaultValueSql("nextval('\"Det\".\"DescriptionKey_DescriptionKeyId_seq\"'::regclass)");

                entity.HasOne(d => d.DescriptionKeyGroup)
                    .WithMany(p => p.DescriptionKey)
                    .HasForeignKey(d => d.DescriptionKeyGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("DescriptionKey_DescriptionKeyGroupId_fkey");

                entity.HasOne(d => d.DescriptionKeyTypeNavigation)
                    .WithMany(p => p.DescriptionKey)
                    .HasForeignKey(d => d.DescriptionKeyType)
                    .HasConstraintName("DescriptionKey_DescriptionKeyType_fkey");

                entity.HasOne(d => d.ParentDescriptionKey)
                    .WithMany(p => p.InverseParentDescriptionKey)
                    .HasForeignKey(d => d.ParentDescriptionKeyId)
                    .HasConstraintName("DescriptionKey_ParentDescriptionKeyId_fkey");
            });

            modelBuilder.Entity<DescriptionKeyGroup>(entity =>
            {
                entity.Property(e => e.DescriptionKeyGroupId).HasDefaultValueSql("nextval('\"Det\".\"DescriptionKeyGroup_DescriptionKeyGroupId_seq\"'::regclass)");

                entity.HasOne(d => d.ParentDescriptionKeyGroup)
                    .WithMany(p => p.InverseParentDescriptionKeyGroup)
                    .HasForeignKey(d => d.ParentDescriptionKeyGroupId)
                    .HasConstraintName("DescriptionKeyGroup_ParentDescriptionKeyGroupId_fkey");

                entity.HasOne(d => d.VisibilityCategory)
                    .WithMany(p => p.DescriptionKeyGroup)
                    .HasForeignKey(d => d.VisibilityCategoryId)
                    .HasConstraintName("DescriptionKeyGroup_VisibilityCategoryId_fkey");
            });

            modelBuilder.Entity<DescriptionKeyType>(entity =>
            {
                entity.Property(e => e.DescriptionKeyTypeId).ValueGeneratedNever();
            });

            modelBuilder.Entity<DescriptionStep>(entity =>
            {
                entity.Property(e => e.DescriptionStepId).HasDefaultValueSql("nextval('\"Det\".\"DescriptionStep_DescriptionStepId_seq\"'::regclass)");

                entity.HasOne(d => d.DescriptionKey)
                    .WithMany(p => p.DescriptionStep)
                    .HasForeignKey(d => d.DescriptionKeyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("DescriptionStep_DescriptionKeyId_fkey");
            });

            modelBuilder.Entity<TaxonDescription>(entity =>
            {
                entity.Property(e => e.TaxonDescriptionId).HasDefaultValueSql("nextval('\"Det\".\"TaxonDescription_TaxonDescriptionId_seq\"'::regclass)");

                entity.HasOne(d => d.DescriptionKey)
                    .WithMany(p => p.TaxonDescription)
                    .HasForeignKey(d => d.DescriptionKeyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TaxonDescription_DescriptionKeyId_fkey");

                entity.HasOne(d => d.DescriptionKeyType)
                    .WithMany(p => p.TaxonDescription)
                    .HasForeignKey(d => d.DescriptionKeyTypeId)
                    .HasConstraintName("TaxonDescription_DescriptionKeyTypeId_fkey");
            });

            modelBuilder.Entity<VisibilityCategory>(entity =>
            {
                entity.ForNpgsqlHasComment("Visibility refering to \"Ampel\" type in Excel");

                entity.Property(e => e.VisibilityCategoryId).ValueGeneratedNever();
            });

            modelBuilder.HasSequence<int>("DescriptionKey_DescriptionKeyId_seq");

            modelBuilder.HasSequence<int>("DescriptionKeyGroup_DescriptionKeyGroupId_seq");

            modelBuilder.HasSequence<int>("DescriptionStep_DescriptionStepId_seq");

            modelBuilder.HasSequence<int>("TaxonDescription_TaxonDescriptionId_seq");
        }
    }
}
