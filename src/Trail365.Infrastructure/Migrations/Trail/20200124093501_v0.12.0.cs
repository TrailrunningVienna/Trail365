using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Trail365.Migrations
{
    public partial class v0120 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blobs",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    FolderName = table.Column<string>(nullable: true),
                    MimeType = table.Column<string>(nullable: true),
                    OriginalFileName = table.Column<string>(nullable: true),
                    ImageHeight = table.Column<int>(nullable: true),
                    ImageWidth = table.Column<int>(nullable: true),
                    StorageSize = table.Column<int>(nullable: true),
                    OwningTrailID = table.Column<Guid>(nullable: true),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedByUser = table.Column<string>(nullable: true),
                    ModifiedUtc = table.Column<DateTime>(nullable: true),
                    ModifiedByUser = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    ContentHash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blobs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Places",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    City = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    Zip = table.Column<string>(nullable: true),
                    Longitude = table.Column<double>(nullable: true),
                    Latitude = table.Column<double>(nullable: true),
                    Radius = table.Column<int>(nullable: true),
                    CountryTwoLetterISOCode = table.Column<string>(nullable: true),
                    CreatedUtc = table.Column<DateTime>(nullable: true),
                    CreatedByUser = table.Column<string>(nullable: true),
                    ModifiedUtc = table.Column<DateTime>(nullable: true),
                    ModifiedByUser = table.Column<string>(nullable: true),
                    ExternalID = table.Column<string>(nullable: true),
                    ExternalSource = table.Column<string>(nullable: true),
                    MeetingPoint = table.Column<string>(nullable: true),
                    IsCityPartOfTheName = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Places", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Trails",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    InternalDescription = table.Column<string>(nullable: true),
                    Excerpt = table.Column<string>(nullable: true),
                    GpxBlobID = table.Column<Guid>(nullable: true),
                    OwnerID = table.Column<Guid>(nullable: true),
                    ExternalID = table.Column<string>(nullable: true),
                    ExternalSource = table.Column<string>(nullable: true),
                    PreviewImageID = table.Column<Guid>(nullable: true),
                    SmallPreviewImageID = table.Column<Guid>(nullable: true),
                    MediumPreviewImageID = table.Column<Guid>(nullable: true),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedByUser = table.Column<string>(nullable: true),
                    ModifiedUtc = table.Column<DateTime>(nullable: true),
                    ModifiedByUser = table.Column<string>(nullable: true),
                    ScrapedUtc = table.Column<DateTime>(nullable: true),
                    GpxDownloadAccess = table.Column<int>(nullable: false),
                    ListAccess = table.Column<int>(nullable: false),
                    DistanceMeters = table.Column<int>(nullable: true),
                    AscentMeters = table.Column<int>(nullable: true),
                    DescentMeters = table.Column<int>(nullable: true),
                    MinimumAltitude = table.Column<int>(nullable: true),
                    MaximumAltitude = table.Column<int>(nullable: true),
                    AltitudeAtStart = table.Column<int>(nullable: true),
                    ElevationProfileImageID = table.Column<Guid>(nullable: true),
                    ElevationProfile_Basic_ImageID = table.Column<Guid>(nullable: true),
                    ElevationProfile_Intermediate_ImageID = table.Column<Guid>(nullable: true),
                    ElevationProfile_Advanced_ImageID = table.Column<Guid>(nullable: true),
                    ElevationProfile_Proficiency_ImageID = table.Column<Guid>(nullable: true),
                    StartPlaceID = table.Column<Guid>(nullable: true),
                    EndPlaceID = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Trail_ElevationProfileImage",
                        column: x => x.ElevationProfileImageID,
                        principalTable: "Blobs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trail_EndPlace",
                        column: x => x.EndPlaceID,
                        principalTable: "Places",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trail_GpxBlob",
                        column: x => x.GpxBlobID,
                        principalTable: "Blobs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trail_MediumPreviewImage",
                        column: x => x.MediumPreviewImageID,
                        principalTable: "Blobs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trail_PreviewImage",
                        column: x => x.PreviewImageID,
                        principalTable: "Blobs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trail_SmallPreviewImage",
                        column: x => x.SmallPreviewImageID,
                        principalTable: "Blobs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trail_StartPlace",
                        column: x => x.StartPlaceID,
                        principalTable: "Places",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    OwnerID = table.Column<Guid>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    ListAccess = table.Column<int>(nullable: false),
                    Excerpt = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedByUser = table.Column<string>(nullable: true),
                    ModifiedUtc = table.Column<DateTime>(nullable: true),
                    ModifiedByUser = table.Column<string>(nullable: true),
                    FullDayEvent = table.Column<bool>(nullable: false),
                    StartTimeUtc = table.Column<DateTime>(nullable: true),
                    EndTimeUtc = table.Column<DateTime>(nullable: true),
                    OrganizerPermalink = table.Column<string>(nullable: true),
                    DescriptionLock = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    InternalDescription = table.Column<string>(nullable: true),
                    PlaceLock = table.Column<int>(nullable: false),
                    PlaceID = table.Column<Guid>(nullable: true),
                    EndPlaceID = table.Column<Guid>(nullable: true),
                    TrailID = table.Column<Guid>(nullable: true),
                    CoverImageID = table.Column<Guid>(nullable: true),
                    ExternalID = table.Column<string>(nullable: true),
                    ExternalSource = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Event_CoverImage",
                        column: x => x.CoverImageID,
                        principalTable: "Blobs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Event_EndPlace",
                        column: x => x.EndPlaceID,
                        principalTable: "Places",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Event_Place",
                        column: x => x.PlaceID,
                        principalTable: "Places",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Event_Trail",
                        column: x => x.TrailID,
                        principalTable: "Trails",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stories",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    ListAccess = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Excerpt = table.Column<string>(nullable: true),
                    CoverImageID = table.Column<Guid>(nullable: true),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedByUser = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    PublishedUtc = table.Column<DateTime>(nullable: true),
                    ModifiedUtc = table.Column<DateTime>(nullable: true),
                    ModifiedByUser = table.Column<string>(nullable: true),
                    EventID = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stories", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Story_CoverImage",
                        column: x => x.CoverImageID,
                        principalTable: "Blobs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stories_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoryBlocks",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    ImageID = table.Column<Guid>(nullable: true),
                    StoryID = table.Column<Guid>(nullable: false),
                    RawContent = table.Column<string>(nullable: true),
                    BlockType = table.Column<int>(nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedByUser = table.Column<string>(nullable: true),
                    ModifiedUtc = table.Column<DateTime>(nullable: true),
                    ModifiedByUser = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    BlockTypeGroup = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryBlocks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StoryBlock_Image",
                        column: x => x.ImageID,
                        principalTable: "Blobs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoryBlock_Story",
                        column: x => x.StoryID,
                        principalTable: "Stories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_CoverImageID",
                table: "Events",
                column: "CoverImageID");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EndPlaceID",
                table: "Events",
                column: "EndPlaceID");

            migrationBuilder.CreateIndex(
                name: "IX_Events_PlaceID",
                table: "Events",
                column: "PlaceID");

            migrationBuilder.CreateIndex(
                name: "IX_Events_TrailID",
                table: "Events",
                column: "TrailID");

            migrationBuilder.CreateIndex(
                name: "IX_Stories_CoverImageID",
                table: "Stories",
                column: "CoverImageID");

            migrationBuilder.CreateIndex(
                name: "IX_Stories_EventID",
                table: "Stories",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlocks_ImageID",
                table: "StoryBlocks",
                column: "ImageID");

            migrationBuilder.CreateIndex(
                name: "IX_StoryBlocks_StoryID",
                table: "StoryBlocks",
                column: "StoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Trails_ElevationProfileImageID",
                table: "Trails",
                column: "ElevationProfileImageID");

            migrationBuilder.CreateIndex(
                name: "IX_Trails_EndPlaceID",
                table: "Trails",
                column: "EndPlaceID");

            migrationBuilder.CreateIndex(
                name: "IX_Trails_GpxBlobID",
                table: "Trails",
                column: "GpxBlobID");

            migrationBuilder.CreateIndex(
                name: "IX_Trails_MediumPreviewImageID",
                table: "Trails",
                column: "MediumPreviewImageID");

            migrationBuilder.CreateIndex(
                name: "IX_Trails_PreviewImageID",
                table: "Trails",
                column: "PreviewImageID");

            migrationBuilder.CreateIndex(
                name: "IX_Trails_SmallPreviewImageID",
                table: "Trails",
                column: "SmallPreviewImageID");

            migrationBuilder.CreateIndex(
                name: "IX_Trails_StartPlaceID",
                table: "Trails",
                column: "StartPlaceID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoryBlocks");

            migrationBuilder.DropTable(
                name: "Stories");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Trails");

            migrationBuilder.DropTable(
                name: "Blobs");

            migrationBuilder.DropTable(
                name: "Places");
        }
    }
}
