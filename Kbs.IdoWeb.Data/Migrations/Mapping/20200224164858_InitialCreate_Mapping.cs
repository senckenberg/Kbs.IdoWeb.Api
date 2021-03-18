using Microsoft.EntityFrameworkCore.Migrations;

namespace Kbs.IdoWeb.Data.Migrations.Mapping
{
    public partial class InitialCreate_Mapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Map");

            migrationBuilder.CreateTable(
                name: "Tk25",
                schema: "Map",
                columns: table => new
                {
                    Tk25Id = table.Column<int>(nullable: false),
                    TkNr = table.Column<int>(nullable: false),
                    TkQuadrant = table.Column<int>(nullable: true),
                    Title = table.Column<string>(maxLength: 200, nullable: true),
                    Wgs84CenterLat = table.Column<decimal>(type: "numeric(18,15)", nullable: true),
                    Wgs84CenterLong = table.Column<decimal>(type: "numeric(18,15)", nullable: true),
                    GkCenterLat = table.Column<decimal>(type: "numeric(18,15)", nullable: true),
                    GkCenterLong = table.Column<decimal>(type: "numeric(18,15)", nullable: true),
                    GkEpsg = table.Column<int>(nullable: true),
                    Tk25IdV2 = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tk25", x => x.Tk25Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tk25",
                schema: "Map");
        }
    }
}
