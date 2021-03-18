using System.Collections;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kbs.IdoWeb.Data.Migrations.Information
{
    public partial class InitialCreate_Information : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Inf");

            migrationBuilder.CreateSequence<int>(
                name: "Region_RegionId_seq");

            migrationBuilder.CreateSequence<int>(
                name: "Taxon_TaxonId_seq");

            migrationBuilder.CreateSequence<int>(
                name: "TaxonToRegion_TaxonRegionId_seq");

            migrationBuilder.CreateTable(
                name: "Region",
                schema: "Inf",
                columns: table => new
                {
                    RegionId = table.Column<int>(nullable: false, defaultValueSql: "nextval('\"Inf\".\"Region_RegionId_seq\"'::regclass)"),
                    SubRegionOfId = table.Column<int>(nullable: true),
                    LocalisationJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Region", x => x.RegionId);
                    table.ForeignKey(
                        name: "Region_SubRegionOfId_fkey",
                        column: x => x.SubRegionOfId,
                        principalSchema: "Inf",
                        principalTable: "Region",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegionState",
                schema: "Inf",
                columns: table => new
                {
                    RegionStateId = table.Column<int>(nullable: false),
                    LocalisationJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionState", x => x.RegionStateId);
                });

            migrationBuilder.CreateTable(
                name: "TaxonomyState",
                schema: "Inf",
                columns: table => new
                {
                    StateId = table.Column<int>(nullable: false),
                    StateLevel = table.Column<int>(nullable: false),
                    StateName = table.Column<string>(maxLength: 50, nullable: false),
                    StateDescription = table.Column<string>(maxLength: 100, nullable: true),
                    IsTreeNode = table.Column<BitArray>(type: "bit(1)", nullable: false),
                    IsMainGroup = table.Column<BitArray>(type: "bit(1)", nullable: false),
                    StateListName = table.Column<string>(maxLength: 100, nullable: true),
                    HierarchyLevel = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TaxonomyState_pkey", x => x.StateId);
                });

            migrationBuilder.CreateTable(
                name: "Taxon",
                schema: "Inf",
                columns: table => new
                {
                    TaxonId = table.Column<int>(nullable: false, defaultValueSql: "nextval('\"Inf\".\"Taxon_TaxonId_seq\"'::regclass)"),
                    KingdomId = table.Column<int>(nullable: true),
                    PhylumId = table.Column<int>(nullable: true),
                    ClassId = table.Column<int>(nullable: true),
                    OrderId = table.Column<int>(nullable: true),
                    FamilyId = table.Column<int>(nullable: true),
                    SubfamilyId = table.Column<int>(nullable: true),
                    GenusId = table.Column<int>(nullable: true),
                    SpeciesId = table.Column<int>(nullable: true),
                    TaxonName = table.Column<string>(maxLength: 100, nullable: false),
                    TaxonDescription = table.Column<string>(nullable: true),
                    DescriptionBy = table.Column<string>(maxLength: 100, nullable: true),
                    DescriptionYear = table.Column<int>(nullable: true),
                    TaxonomyStateId = table.Column<int>(nullable: true),
                    Diagnose = table.Column<string>(nullable: true),
                    IdentificationLevelMale = table.Column<int>(nullable: true),
                    IdentificationLevelFemale = table.Column<int>(nullable: true),
                    TaxonDistribution = table.Column<string>(nullable: true),
                    TaxonBiotopeAndLifestyle = table.Column<string>(nullable: true),
                    LocalisationJson = table.Column<string>(type: "jsonb", nullable: true),
                    SubphylumId = table.Column<int>(nullable: true),
                    SubclassId = table.Column<int>(nullable: true),
                    SuborderId = table.Column<int>(nullable: true),
                    HasBracketDescription = table.Column<bool>(nullable: true, defaultValueSql: "false"),
                    HasTaxDescChildren = table.Column<bool>(nullable: true, defaultValueSql: "false"),
                    Group = table.Column<string>(nullable: true),
                    EdaphobaseId = table.Column<int>(nullable: true),
                    Synonyms = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taxon", x => x.TaxonId);
                    table.ForeignKey(
                        name: "Taxon_ClassId_fkey",
                        column: x => x.ClassId,
                        principalSchema: "Inf",
                        principalTable: "Taxon",
                        principalColumn: "TaxonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "Taxon_FamilyId_fkey",
                        column: x => x.FamilyId,
                        principalSchema: "Inf",
                        principalTable: "Taxon",
                        principalColumn: "TaxonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "Taxon_GenusId_fkey",
                        column: x => x.GenusId,
                        principalSchema: "Inf",
                        principalTable: "Taxon",
                        principalColumn: "TaxonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "Taxon_KingdomId_fkey",
                        column: x => x.KingdomId,
                        principalSchema: "Inf",
                        principalTable: "Taxon",
                        principalColumn: "TaxonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "Taxon_OrderId_fkey",
                        column: x => x.OrderId,
                        principalSchema: "Inf",
                        principalTable: "Taxon",
                        principalColumn: "TaxonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "Taxon_PhylumId_fkey",
                        column: x => x.PhylumId,
                        principalSchema: "Inf",
                        principalTable: "Taxon",
                        principalColumn: "TaxonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "Taxon_SpeciesId_fkey",
                        column: x => x.SpeciesId,
                        principalSchema: "Inf",
                        principalTable: "Taxon",
                        principalColumn: "TaxonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "Taxon_SubclassId_fkey",
                        column: x => x.SubclassId,
                        principalSchema: "Inf",
                        principalTable: "Taxon",
                        principalColumn: "TaxonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "Taxon_SubfamilyId_fkey",
                        column: x => x.SubfamilyId,
                        principalSchema: "Inf",
                        principalTable: "Taxon",
                        principalColumn: "TaxonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "Taxon_SuborderId_fkey",
                        column: x => x.SuborderId,
                        principalSchema: "Inf",
                        principalTable: "Taxon",
                        principalColumn: "TaxonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "Taxon_SubphylumId_fkey",
                        column: x => x.SubphylumId,
                        principalSchema: "Inf",
                        principalTable: "Taxon",
                        principalColumn: "TaxonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "Taxon_TaxonomyStateId_fkey",
                        column: x => x.TaxonomyStateId,
                        principalSchema: "Inf",
                        principalTable: "TaxonomyState",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxonToRegion",
                schema: "Inf",
                columns: table => new
                {
                    TaxonRegionId = table.Column<int>(nullable: false, defaultValueSql: "nextval('\"Inf\".\"TaxonToRegion_TaxonRegionId_seq\"'::regclass)"),
                    RegionId = table.Column<int>(nullable: false),
                    TaxonId = table.Column<int>(nullable: false),
                    RegionStateId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TaxonToRegion_pkey", x => x.TaxonRegionId);
                    table.ForeignKey(
                        name: "TaxonToRegion_RegionId_fkey",
                        column: x => x.RegionId,
                        principalSchema: "Inf",
                        principalTable: "Region",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "TaxonToRegion_TaxonId_fkey",
                        column: x => x.TaxonId,
                        principalSchema: "Inf",
                        principalTable: "Taxon",
                        principalColumn: "TaxonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Region_SubRegionOfId",
                schema: "Inf",
                table: "Region",
                column: "SubRegionOfId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxon_ClassId",
                schema: "Inf",
                table: "Taxon",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxon_FamilyId",
                schema: "Inf",
                table: "Taxon",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxon_GenusId",
                schema: "Inf",
                table: "Taxon",
                column: "GenusId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxon_KingdomId",
                schema: "Inf",
                table: "Taxon",
                column: "KingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxon_OrderId",
                schema: "Inf",
                table: "Taxon",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxon_PhylumId",
                schema: "Inf",
                table: "Taxon",
                column: "PhylumId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxon_SpeciesId",
                schema: "Inf",
                table: "Taxon",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxon_SubclassId",
                schema: "Inf",
                table: "Taxon",
                column: "SubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxon_SubfamilyId",
                schema: "Inf",
                table: "Taxon",
                column: "SubfamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxon_SuborderId",
                schema: "Inf",
                table: "Taxon",
                column: "SuborderId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxon_SubphylumId",
                schema: "Inf",
                table: "Taxon",
                column: "SubphylumId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxon_TaxonomyStateId",
                schema: "Inf",
                table: "Taxon",
                column: "TaxonomyStateId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxonToRegion_RegionId",
                schema: "Inf",
                table: "TaxonToRegion",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxonToRegion_TaxonId",
                schema: "Inf",
                table: "TaxonToRegion",
                column: "TaxonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegionState",
                schema: "Inf");

            migrationBuilder.DropTable(
                name: "TaxonToRegion",
                schema: "Inf");

            migrationBuilder.DropTable(
                name: "Region",
                schema: "Inf");

            migrationBuilder.DropTable(
                name: "Taxon",
                schema: "Inf");

            migrationBuilder.DropTable(
                name: "TaxonomyState",
                schema: "Inf");

            migrationBuilder.DropSequence(
                name: "Region_RegionId_seq");

            migrationBuilder.DropSequence(
                name: "Taxon_TaxonId_seq");

            migrationBuilder.DropSequence(
                name: "TaxonToRegion_TaxonRegionId_seq");
        }
    }
}
