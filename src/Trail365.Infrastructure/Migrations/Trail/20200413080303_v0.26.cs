using Microsoft.EntityFrameworkCore.Migrations;

namespace Trail365.Migrations
{
    public partial class v026 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BoundingBox",
                table: "Trails",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoundingBox",
                table: "Trails");
        }
    }
}
