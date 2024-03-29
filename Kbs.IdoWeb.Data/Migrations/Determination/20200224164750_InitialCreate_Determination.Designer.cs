﻿// <auto-generated />
using System;
using Kbs.IdoWeb.Data.Determination;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Kbs.IdoWeb.Data.Migrations.Determination
{
    [DbContext(typeof(DeterminationContext))]
    [Migration("20200224164750_InitialCreate_Determination")]
    partial class InitialCreate_Determination
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("Relational:Sequence:.DescriptionKey_DescriptionKeyId_seq", "'DescriptionKey_DescriptionKeyId_seq', '', '1', '1', '', '', 'Int32', 'False'")
                .HasAnnotation("Relational:Sequence:.DescriptionKeyGroup_DescriptionKeyGroupId_seq", "'DescriptionKeyGroup_DescriptionKeyGroupId_seq', '', '1', '1', '', '', 'Int32', 'False'")
                .HasAnnotation("Relational:Sequence:.DescriptionStep_DescriptionStepId_seq", "'DescriptionStep_DescriptionStepId_seq', '', '1', '1', '', '', 'Int32', 'False'")
                .HasAnnotation("Relational:Sequence:.TaxonDescription_TaxonDescriptionId_seq", "'TaxonDescription_TaxonDescriptionId_seq', '', '1', '1', '', '', 'Int32', 'False'");

            modelBuilder.Entity("Kbs.IdoWeb.Data.Determination.DescriptionKey", b =>
                {
                    b.Property<int>("DescriptionKeyId")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("nextval('\"Det\".\"DescriptionKey_DescriptionKeyId_seq\"'::regclass)");

                    b.Property<int>("DescriptionKeyGroupId");

                    b.Property<int?>("DescriptionKeyType");

                    b.Property<string>("KeyDescription");

                    b.Property<string>("KeyName");

                    b.Property<string>("ListSourceJson")
                        .HasColumnType("jsonb");

                    b.Property<string>("LocalisationJson")
                        .HasColumnType("jsonb");

                    b.Property<int?>("ParentDescriptionKeyId");

                    b.HasKey("DescriptionKeyId");

                    b.HasIndex("DescriptionKeyGroupId");

                    b.HasIndex("DescriptionKeyType");

                    b.HasIndex("ParentDescriptionKeyId");

                    b.ToTable("DescriptionKey","Det");
                });

            modelBuilder.Entity("Kbs.IdoWeb.Data.Determination.DescriptionKeyGroup", b =>
                {
                    b.Property<int>("DescriptionKeyGroupId")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("nextval('\"Det\".\"DescriptionKeyGroup_DescriptionKeyGroupId_seq\"'::regclass)");

                    b.Property<string>("DescriptionKeyGroupDataType")
                        .HasMaxLength(10);

                    b.Property<string>("DescriptionKeyGroupType")
                        .HasColumnType("jsonb");

                    b.Property<string>("KeyGroupName");

                    b.Property<string>("LocalisationJson")
                        .HasColumnType("jsonb");

                    b.Property<int?>("ParentDescriptionKeyGroupId");

                    b.Property<int?>("VisibilityCategoryId");

                    b.HasKey("DescriptionKeyGroupId");

                    b.HasIndex("ParentDescriptionKeyGroupId");

                    b.HasIndex("VisibilityCategoryId");

                    b.ToTable("DescriptionKeyGroup","Det");
                });

            modelBuilder.Entity("Kbs.IdoWeb.Data.Determination.DescriptionKeyType", b =>
                {
                    b.Property<int>("DescriptionKeyTypeId");

                    b.Property<string>("DescriptionKeyTypeName")
                        .HasMaxLength(30);

                    b.Property<string>("LocalisationJson")
                        .HasColumnType("jsonb");

                    b.HasKey("DescriptionKeyTypeId");

                    b.ToTable("DescriptionKeyType","Det");
                });

            modelBuilder.Entity("Kbs.IdoWeb.Data.Determination.DescriptionStep", b =>
                {
                    b.Property<int>("DescriptionStepId")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("nextval('\"Det\".\"DescriptionStep_DescriptionStepId_seq\"'::regclass)");

                    b.Property<int>("BaseTaxonId");

                    b.Property<int>("DescriptionKeyId");

                    b.Property<int>("StepOrder");

                    b.HasKey("DescriptionStepId");

                    b.HasIndex("DescriptionKeyId");

                    b.ToTable("DescriptionStep","Det");
                });

            modelBuilder.Entity("Kbs.IdoWeb.Data.Determination.TaxonDescription", b =>
                {
                    b.Property<int>("TaxonDescriptionId")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("nextval('\"Det\".\"TaxonDescription_TaxonDescriptionId_seq\"'::regclass)");

                    b.Property<int>("DescriptionKeyId");

                    b.Property<int?>("DescriptionKeyTypeId");

                    b.Property<string>("KeyValue")
                        .HasMaxLength(100);

                    b.Property<decimal?>("MaxValue")
                        .HasColumnType("numeric(7,2)");

                    b.Property<decimal?>("MinValue")
                        .HasColumnType("numeric(7,2)");

                    b.Property<int>("TaxonId");

                    b.HasKey("TaxonDescriptionId");

                    b.HasIndex("DescriptionKeyId");

                    b.HasIndex("DescriptionKeyTypeId");

                    b.ToTable("TaxonDescription","Det");
                });

            modelBuilder.Entity("Kbs.IdoWeb.Data.Determination.VisibilityCategory", b =>
                {
                    b.Property<int>("VisibilityCategoryId");

                    b.Property<string>("VisibilityCategoryName");

                    b.HasKey("VisibilityCategoryId");

                    b.ToTable("VisibilityCategory","Det");

                    b.HasAnnotation("Npgsql:Comment", "Visibility refering to \"Ampel\" type in Excel");
                });

            modelBuilder.Entity("Kbs.IdoWeb.Data.Determination.DescriptionKey", b =>
                {
                    b.HasOne("Kbs.IdoWeb.Data.Determination.DescriptionKeyGroup", "DescriptionKeyGroup")
                        .WithMany("DescriptionKey")
                        .HasForeignKey("DescriptionKeyGroupId")
                        .HasConstraintName("DescriptionKey_DescriptionKeyGroupId_fkey");

                    b.HasOne("Kbs.IdoWeb.Data.Determination.DescriptionKeyType", "DescriptionKeyTypeNavigation")
                        .WithMany("DescriptionKey")
                        .HasForeignKey("DescriptionKeyType")
                        .HasConstraintName("DescriptionKey_DescriptionKeyType_fkey");

                    b.HasOne("Kbs.IdoWeb.Data.Determination.DescriptionKey", "ParentDescriptionKey")
                        .WithMany("InverseParentDescriptionKey")
                        .HasForeignKey("ParentDescriptionKeyId")
                        .HasConstraintName("DescriptionKey_ParentDescriptionKeyId_fkey");
                });

            modelBuilder.Entity("Kbs.IdoWeb.Data.Determination.DescriptionKeyGroup", b =>
                {
                    b.HasOne("Kbs.IdoWeb.Data.Determination.DescriptionKeyGroup", "ParentDescriptionKeyGroup")
                        .WithMany("InverseParentDescriptionKeyGroup")
                        .HasForeignKey("ParentDescriptionKeyGroupId")
                        .HasConstraintName("DescriptionKeyGroup_ParentDescriptionKeyGroupId_fkey");

                    b.HasOne("Kbs.IdoWeb.Data.Determination.VisibilityCategory", "VisibilityCategory")
                        .WithMany("DescriptionKeyGroup")
                        .HasForeignKey("VisibilityCategoryId")
                        .HasConstraintName("DescriptionKeyGroup_VisibilityCategoryId_fkey");
                });

            modelBuilder.Entity("Kbs.IdoWeb.Data.Determination.DescriptionStep", b =>
                {
                    b.HasOne("Kbs.IdoWeb.Data.Determination.DescriptionKey", "DescriptionKey")
                        .WithMany("DescriptionStep")
                        .HasForeignKey("DescriptionKeyId")
                        .HasConstraintName("DescriptionStep_DescriptionKeyId_fkey");
                });

            modelBuilder.Entity("Kbs.IdoWeb.Data.Determination.TaxonDescription", b =>
                {
                    b.HasOne("Kbs.IdoWeb.Data.Determination.DescriptionKey", "DescriptionKey")
                        .WithMany("TaxonDescription")
                        .HasForeignKey("DescriptionKeyId")
                        .HasConstraintName("TaxonDescription_DescriptionKeyId_fkey");

                    b.HasOne("Kbs.IdoWeb.Data.Determination.DescriptionKeyType", "DescriptionKeyType")
                        .WithMany("TaxonDescription")
                        .HasForeignKey("DescriptionKeyTypeId")
                        .HasConstraintName("TaxonDescription_DescriptionKeyTypeId_fkey");
                });
#pragma warning restore 612, 618
        }
    }
}
