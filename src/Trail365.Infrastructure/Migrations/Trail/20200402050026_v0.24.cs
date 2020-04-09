using Microsoft.EntityFrameworkCore.Migrations;

namespace Trail365.Migrations
{
    public partial class v024 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AsphaltedRoadMeters",
                table: "Trails",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PavedRoadMeters",
                table: "Trails",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnclassifiedMeters",
                table: "Trails",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnpavedTrailMeters",
                table: "Trails",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsphaltedRoadMeters",
                table: "Trails");

            migrationBuilder.DropColumn(
                name: "PavedRoadMeters",
                table: "Trails");

            migrationBuilder.DropColumn(
                name: "UnclassifiedMeters",
                table: "Trails");

            migrationBuilder.DropColumn(
                name: "UnpavedTrailMeters",
                table: "Trails");
        }
    }
}
