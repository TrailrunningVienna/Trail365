using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Trail365.Migrations.Identity
{
    public partial class v0100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    AvatarUrl = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Surname = table.Column<string>(nullable: true),
                    GivenName = table.Column<string>(nullable: true),
                    UserRoles = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Identities",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    UserID = table.Column<Guid>(nullable: false),
                    Identifier = table.Column<string>(nullable: true),
                    AuthenticationType = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    CreatedUtc = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identities", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FederatedIdentity_User",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Identities_UserID",
                table: "Identities",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Identities_Identifier_AuthenticationType",
                table: "Identities",
                columns: new[] { "Identifier", "AuthenticationType" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Identities");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
