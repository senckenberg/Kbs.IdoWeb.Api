using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kbs.IdoWeb.Data.Observation
{
    public partial class ObservationContext : DbContext
    {
        public ObservationContext()
        {
        }

        public ObservationContext(DbContextOptions<ObservationContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AccuracyType> AccuracyType { get; set; }
        public virtual DbSet<ApprovalState> ApprovalState { get; set; }
        public virtual DbSet<DiagnosisType> DiagnosisType { get; set; }
        public virtual DbSet<Event> Event { get; set; }
        public virtual DbSet<HabitatType> HabitatType { get; set; }
        public virtual DbSet<Image> Image { get; set; }
        public virtual DbSet<ImageLicense> ImageLicense { get; set; }
        public virtual DbSet<ImageTag> ImageTag { get; set; }
        public virtual DbSet<ImageTagGroup> ImageTagGroup { get; set; }
        public virtual DbSet<ImageToTag> ImageToTag { get; set; }
        public virtual DbSet<LastSyncVersion> LastSyncVersion { get; set; }
        public virtual DbSet<LocalityType> LocalityType { get; set; }
        public virtual DbSet<Observation> Observation { get; set; }
        public virtual DbSet<SizeGroup> SizeGroup { get; set; }

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

            modelBuilder.Entity<AccuracyType>(entity =>
            {
                entity.Property(e => e.AccuracyTypeId).ValueGeneratedNever();
            });

            modelBuilder.Entity<ApprovalState>(entity =>
            {
                entity.Property(e => e.ApprovalStateId).ValueGeneratedNever();
            });

            modelBuilder.Entity<DiagnosisType>(entity =>
            {
                entity.Property(e => e.DiagnosisTypeId).ValueGeneratedNever();
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.Property(e => e.EventId).HasDefaultValueSql("nextval('\"Obs\".\"Event_EventId_seq\"'::regclass)");

                entity.HasOne(d => d.Accuracy)
                    .WithMany(p => p.Event)
                    .HasForeignKey(d => d.AccuracyId)
                    .HasConstraintName("Event_AccuracyId_fkey");

                entity.HasOne(d => d.ApprovalState)
                    .WithMany(p => p.Event)
                    .HasForeignKey(d => d.ApprovalStateId)
                    .HasConstraintName("Event_ApprovalStateId_fkey");

                entity.HasOne(d => d.HabitatType)
                    .WithMany(p => p.Event)
                    .HasForeignKey(d => d.HabitatTypeId)
                    .HasConstraintName("Event_HabitatTypeId_fkey");
            });

            modelBuilder.Entity<HabitatType>(entity =>
            {
                entity.Property(e => e.HabitatTypeId).ValueGeneratedNever();
            });

            modelBuilder.Entity<Image>(entity =>
            {
                entity.Property(e => e.ImageId).HasDefaultValueSql("nextval('\"Obs\".\"Image_ImageId_seq\"'::regclass)");

                entity.Property(e => e.CmsId).ForNpgsqlHasComment("Identifier for Image in CMS, for Wordpress == attachment_id");

                entity.HasOne(d => d.License)
                    .WithMany(p => p.Image)
                    .HasForeignKey(d => d.LicenseId)
                    .HasConstraintName("Image_LicenseId_fkey");

                entity.HasOne(d => d.Observation)
                    .WithMany(p => p.Image)
                    .HasForeignKey(d => d.ObservationId)
                    .HasConstraintName("Image_ObservationId_fkey");
            });

            modelBuilder.Entity<ImageLicense>(entity =>
            {
                entity.HasKey(e => e.LicenseId)
                    .HasName("ImageLicense_pkey");

                entity.Property(e => e.LicenseId).ValueGeneratedNever();
            });

            modelBuilder.Entity<ImageTag>(entity =>
            {
                entity.Property(e => e.ImageTagId).HasDefaultValueSql("nextval('\"Obs\".\"ImageTag_ImageTagId_seq\"'::regclass)");

                entity.HasOne(d => d.ImageTagGroup)
                    .WithMany(p => p.ImageTag)
                    .HasForeignKey(d => d.ImageTagGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ImageTag_ImageTagGroupId_fkey");
            });

            modelBuilder.Entity<ImageTagGroup>(entity =>
            {
                entity.Property(e => e.ImageTagGroupId).HasDefaultValueSql("nextval('\"Obs\".\"ImageTagGroup_ImageTagGroupId_seq\"'::regclass)");
            });

            modelBuilder.Entity<ImageToTag>(entity =>
            {
                entity.Property(e => e.ImageToTagId).HasDefaultValueSql("nextval('\"Obs\".\"ImageToTag_ImageToTagId_seq\"'::regclass)");
            });

            modelBuilder.Entity<LastSyncVersion>(entity =>
            {
                entity.HasKey(e => e.SyncTypeName)
                    .HasName("LastSyncVersion_pkey");

                entity.Property(e => e.SyncTypeName).ValueGeneratedNever();
            });

            modelBuilder.Entity<LocalityType>(entity =>
            {
                entity.Property(e => e.LocalityTypeId).ValueGeneratedNever();
            });

            modelBuilder.Entity<Observation>(entity =>
            {
                entity.HasIndex(e => e.TaxonId)
                    .HasName("fki_Observation_Taxon_fkey");

                entity.Property(e => e.ObservationId).HasDefaultValueSql("nextval('\"Obs\".\"Observation_ObservationId_seq\"'::regclass)");

                entity.Property(e => e.ApprovalStateId).HasDefaultValueSql("1");

                entity.HasOne(d => d.ApprovalState)
                    .WithMany(p => p.Observation)
                    .HasForeignKey(d => d.ApprovalStateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Observation_ApprovalStateId_fkey");

                entity.HasOne(d => d.DiagnosisType)
                    .WithMany(p => p.Observation)
                    .HasForeignKey(d => d.DiagnosisTypeId)
                    .HasConstraintName("Observation_DiagnosisTypeId_fkey");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.Observation)
                    .HasForeignKey(d => d.EventId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Observation_EventId_fkey");

                entity.HasOne(d => d.LocalityType)
                    .WithMany(p => p.Observation)
                    .HasForeignKey(d => d.LocalityTypeId)
                    .HasConstraintName("Observation_LocalityTypeId_fkey");

                entity.HasOne(d => d.SizeGroup)
                    .WithMany(p => p.Observation)
                    .HasForeignKey(d => d.SizeGroupId)
                    .HasConstraintName("Observation_SizeGroupId_fkey");
            });

            modelBuilder.Entity<SizeGroup>(entity =>
            {
                entity.Property(e => e.SizeGroupId).ValueGeneratedNever();
            });

            modelBuilder.HasSequence<int>("Event_EventId_seq");

            modelBuilder.HasSequence<int>("Image_ImageId_seq");

            modelBuilder.HasSequence<int>("ImageTag_ImageTagId_seq");

            modelBuilder.HasSequence<int>("ImageTagGroup_ImageTagGroupId_seq");

            modelBuilder.HasSequence<int>("ImageToTag_ImageToTagId_seq");

            modelBuilder.HasSequence<int>("Observation_ObservationId_seq");
        }
    }
}
