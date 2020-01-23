using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Trail365.Migrations.Task
{
    public partial class v0100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskLogItems",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    TimestampUtc = table.Column<DateTime>(nullable: false),
                    LogMessage = table.Column<string>(nullable: true),
                    Level = table.Column<string>(nullable: true),
                    Category = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskLogItems", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskLogItems");
        }
    }
}
