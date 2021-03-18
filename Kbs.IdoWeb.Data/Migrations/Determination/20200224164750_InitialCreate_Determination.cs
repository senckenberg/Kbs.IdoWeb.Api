using Microsoft.EntityFrameworkCore.Migrations;

namespace Kbs.IdoWeb.Data.Migrations.Determination
{
    public partial class InitialCreate_Determination : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Det");

            migrationBuilder.CreateSequence<int>(
                name: "DescriptionKey_DescriptionKeyId_seq");

            migrationBuilder.CreateSequence<int>(
                name: "DescriptionKeyGroup_DescriptionKeyGroupId_seq");

            migrationBuilder.CreateSequence<int>(
                name: "DescriptionStep_DescriptionStepId_seq");

            migrationBuilder.CreateSequence<int>(
                name: "TaxonDescription_TaxonDescriptionId_seq");

            migrationBuilder.CreateTable(
                name: "DescriptionKeyType",
                schema: "Det",
                columns: table => new
                {
                    DescriptionKeyTypeId = table.Column<int>(nullable: false),
                    DescriptionKeyTypeName = table.Column<string>(maxLength: 30, nullable: true),
                    LocalisationJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescriptionKeyType", x => x.DescriptionKeyTypeId);
                });

            migrationBuilder.CreateTable(
                name: "VisibilityCategory",
                schema: "Det",
                columns: table => new
                {
                    VisibilityCategoryId = table.Column<int>(nullable: false),
                    VisibilityCategoryName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisibilityCategory", x => x.VisibilityCategoryId);
                })
                .Annotation("Npgsql:Comment", "Visibility refering to \"Ampel\" type in Excel");

            migrationBuilder.CreateTable(
                name: "DescriptionKeyGroup",
                schema: "Det",
                columns: table => new
                {
                    DescriptionKeyGroupId = table.Column<int>(nullable: false, defaultValueSql: "nextval('\"Det\".\"DescriptionKeyGroup_DescriptionKeyGroupId_seq\"'::regclass)"),
                    KeyGroupName = table.Column<string>(nullable: true),
                    DescriptionKeyGroupDataType = table.Column<string>(maxLength: 10, nullable: true),
                    LocalisationJson = table.Column<string>(type: "jsonb", nullable: true),
                    ParentDescriptionKeyGroupId = table.Column<int>(nullable: true),
                    DescriptionKeyGroupType = table.Column<string>(type: "jsonb", nullable: true),
                    VisibilityCategoryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescriptionKeyGroup", x => x.DescriptionKeyGroupId);
                    table.ForeignKey(
                        name: "DescriptionKeyGroup_ParentDescriptionKeyGroupId_fkey",
                        column: x => x.ParentDescriptionKeyGroupId,
                        principalSchema: "Det",
                        principalTable: "DescriptionKeyGroup",
                        principalColumn: "DescriptionKeyGroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "DescriptionKeyGroup_VisibilityCategoryId_fkey",
                        column: x => x.VisibilityCategoryId,
                        principalSchema: "Det",
                        principalTable: "VisibilityCategory",
                        principalColumn: "VisibilityCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DescriptionKey",
                schema: "Det",
                columns: table => new
                {
                    DescriptionKeyId = table.Column<int>(nullable: false, defaultValueSql: "nextval('\"Det\".\"DescriptionKey_DescriptionKeyId_seq\"'::regclass)"),
                    DescriptionKeyGroupId = table.Column<int>(nullable: false),
                    KeyName = table.Column<string>(nullable: true),
                    KeyDescription = table.Column<string>(nullable: true),
                    ParentDescriptionKeyId = table.Column<int>(nullable: true),
                    ListSourceJson = table.Column<string>(type: "jsonb", nullable: true),
                    LocalisationJson = table.Column<string>(type: "jsonb", nullable: true),
                    DescriptionKeyType = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescriptionKey", x => x.DescriptionKeyId);
                    table.ForeignKey(
                        name: "DescriptionKey_DescriptionKeyGroupId_fkey",
                        column: x => x.DescriptionKeyGroupId,
                        principalSchema: "Det",
                        principalTable: "DescriptionKeyGroup",
                        principalColumn: "DescriptionKeyGroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "DescriptionKey_DescriptionKeyType_fkey",
                        column: x => x.DescriptionKeyType,
                        principalSchema: "Det",
                        principalTable: "DescriptionKeyType",
                        principalColumn: "DescriptionKeyTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "DescriptionKey_ParentDescriptionKeyId_fkey",
                        column: x => x.ParentDescriptionKeyId,
                        principalSchema: "Det",
                        principalTable: "DescriptionKey",
                        principalColumn: "DescriptionKeyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DescriptionStep",
                schema: "Det",
                columns: table => new
                {
                    DescriptionStepId = table.Column<int>(nullable: false, defaultValueSql: "nextval('\"Det\".\"DescriptionStep_DescriptionStepId_seq\"'::regclass)"),
                    BaseTaxonId = table.Column<int>(nullable: false),
                    DescriptionKeyId = table.Column<int>(nullable: false),
                    StepOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescriptionStep", x => x.DescriptionStepId);
                    table.ForeignKey(
                        name: "DescriptionStep_DescriptionKeyId_fkey",
                        column: x => x.DescriptionKeyId,
                        principalSchema: "Det",
                        principalTable: "DescriptionKey",
                        principalColumn: "DescriptionKeyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxonDescription",
                schema: "Det",
                columns: table => new
                {
                    TaxonDescriptionId = table.Column<int>(nullable: false, defaultValueSql: "nextval('\"Det\".\"TaxonDescription_TaxonDescriptionId_seq\"'::regclass)"),
                    TaxonId = table.Column<int>(nullable: false),
                    DescriptionKeyId = table.Column<int>(nullable: false),
                    KeyValue = table.Column<string>(maxLength: 100, nullable: true),
                    DescriptionKeyTypeId = table.Column<int>(nullable: true),
                    MinValue = table.Column<decimal>(type: "numeric(7,2)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "numeric(7,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxonDescription", x => x.TaxonDescriptionId);
                    table.ForeignKey(
                        name: "TaxonDescription_DescriptionKeyId_fkey",
                        column: x => x.DescriptionKeyId,
                        principalSchema: "Det",
                        principalTable: "DescriptionKey",
                        principalColumn: "DescriptionKeyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "TaxonDescription_DescriptionKeyTypeId_fkey",
                        column: x => x.DescriptionKeyTypeId,
                        principalSchema: "Det",
                        principalTable: "DescriptionKeyType",
                        principalColumn: "DescriptionKeyTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionKey_DescriptionKeyGroupId",
                schema: "Det",
                table: "DescriptionKey",
                column: "DescriptionKeyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionKey_DescriptionKeyType",
                schema: "Det",
                table: "DescriptionKey",
                column: "DescriptionKeyType");

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionKey_ParentDescriptionKeyId",
                schema: "Det",
                table: "DescriptionKey",
                column: "ParentDescriptionKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionKeyGroup_ParentDescriptionKeyGroupId",
                schema: "Det",
                table: "DescriptionKeyGroup",
                column: "ParentDescriptionKeyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionKeyGroup_VisibilityCategoryId",
                schema: "Det",
                table: "DescriptionKeyGroup",
                column: "VisibilityCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionStep_DescriptionKeyId",
                schema: "Det",
                table: "DescriptionStep",
                column: "DescriptionKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxonDescription_DescriptionKeyId",
                schema: "Det",
                table: "TaxonDescription",
                column: "DescriptionKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxonDescription_DescriptionKeyTypeId",
                schema: "Det",
                table: "TaxonDescription",
                column: "DescriptionKeyTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DescriptionStep",
                schema: "Det");

            migrationBuilder.DropTable(
                name: "TaxonDescription",
                schema: "Det");

            migrationBuilder.DropTable(
                name: "DescriptionKey",
                schema: "Det");

            migrationBuilder.DropTable(
                name: "DescriptionKeyGroup",
                schema: "Det");

            migrationBuilder.DropTable(
                name: "DescriptionKeyType",
                schema: "Det");

            migrationBuilder.DropTable(
                name: "VisibilityCategory",
                schema: "Det");

            migrationBuilder.DropSequence(
                name: "DescriptionKey_DescriptionKeyId_seq");

            migrationBuilder.DropSequence(
                name: "DescriptionKeyGroup_DescriptionKeyGroupId_seq");

            migrationBuilder.DropSequence(
                name: "DescriptionStep_DescriptionStepId_seq");

            migrationBuilder.DropSequence(
                name: "TaxonDescription_TaxonDescriptionId_seq");
        }
    }
}
