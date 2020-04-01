using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Trail365.Migrations
{
    public partial class v023 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
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
                    table.PrimaryKey("PK_User", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EventInvolvements",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    EventID = table.Column<Guid>(nullable: false),
                    UserID = table.Column<Guid>(nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    InvitedByUserID = table.Column<Guid>(nullable: true),
                    CreatedByUser = table.Column<string>(nullable: true),
                    ModifiedUtc = table.Column<DateTime>(nullable: true),
                    ModifiedByUser = table.Column<string>(nullable: true),
                    Level = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventInvolvements", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EventInvolvements_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventInvolvement_User",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FederatedIdentity",
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
                    table.PrimaryKey("PK_FederatedIdentity", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FederatedIdentity_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventInvolvements_EventID",
                table: "EventInvolvements",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_EventInvolvements_UserID",
                table: "EventInvolvements",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_FederatedIdentity_UserID",
                table: "FederatedIdentity",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventInvolvements");

            migrationBuilder.DropTable(
                name: "FederatedIdentity");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
