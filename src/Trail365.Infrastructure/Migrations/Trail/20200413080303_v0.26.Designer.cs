﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Trail365.Data;

namespace Trail365.Migrations
{
    [DbContext(typeof(TrailContext))]
    [Migration("20200413080303_v0.26")]
    partial class v026
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3");

            modelBuilder.Entity("Trail365.Entities.Blob", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ContentHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("FolderName")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ImageHeight")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ImageWidth")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MimeType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ModifiedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ModifiedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalFileName")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("OwningTrailID")
                        .HasColumnType("TEXT");

                    b.Property<int?>("StorageSize")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Blobs");
                });

            modelBuilder.Entity("Trail365.Entities.Event", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("CoverImageID")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<int>("DescriptionLock")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("EndPlaceID")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("EndTimeUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("Excerpt")
                        .HasColumnType("TEXT");

                    b.Property<string>("ExternalID")
                        .HasColumnType("TEXT");

                    b.Property<string>("ExternalSource")
                        .HasColumnType("TEXT");

                    b.Property<bool>("FullDayEvent")
                        .HasColumnType("INTEGER");

                    b.Property<string>("InternalDescription")
                        .HasColumnType("TEXT");

                    b.Property<int>("ListAccess")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ModifiedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ModifiedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OrganizerPermalink")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("OwnerID")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("PlaceID")
                        .HasColumnType("TEXT");

                    b.Property<int>("PlaceLock")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("StartTimeUtc")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("TrailID")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("CoverImageID");

                    b.HasIndex("EndPlaceID");

                    b.HasIndex("PlaceID");

                    b.HasIndex("TrailID");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("Trail365.Entities.EventInvolvement", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("EventID")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("InvitedByUserID")
                        .HasColumnType("TEXT");

                    b.Property<int>("Level")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ModifiedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ModifiedUtc")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserID")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("EventID");

                    b.HasIndex("UserID");

                    b.ToTable("EventInvolvements");
                });

            modelBuilder.Entity("Trail365.Entities.FederatedIdentity", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AuthenticationType")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<string>("Identifier")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserID")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("FederatedIdentity");
                });

            modelBuilder.Entity("Trail365.Entities.Place", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("City")
                        .HasColumnType("TEXT");

                    b.Property<string>("Country")
                        .HasColumnType("TEXT");

                    b.Property<string>("CountryTwoLetterISOCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("ExternalID")
                        .HasColumnType("TEXT");

                    b.Property<string>("ExternalSource")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsCityPartOfTheName")
                        .HasColumnType("INTEGER");

                    b.Property<double?>("Latitude")
                        .HasColumnType("REAL");

                    b.Property<double?>("Longitude")
                        .HasColumnType("REAL");

                    b.Property<string>("MeetingPoint")
                        .HasColumnType("TEXT");

                    b.Property<string>("ModifiedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ModifiedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("Radius")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Zip")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Places");
                });

            modelBuilder.Entity("Trail365.Entities.Story", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("CoverImageID")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("EventID")
                        .HasColumnType("TEXT");

                    b.Property<string>("Excerpt")
                        .HasColumnType("TEXT");

                    b.Property<int>("ListAccess")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ModifiedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ModifiedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("PublishedUtc")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("CoverImageID");

                    b.HasIndex("EventID");

                    b.ToTable("Stories");
                });

            modelBuilder.Entity("Trail365.Entities.StoryBlock", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("BlockType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BlockTypeGroup")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ImageID")
                        .HasColumnType("TEXT");

                    b.Property<string>("ModifiedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ModifiedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("RawContent")
                        .HasColumnType("TEXT");

                    b.Property<int>("SortOrder")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("StoryID")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ImageID");

                    b.HasIndex("StoryID");

                    b.ToTable("StoryBlocks");
                });

            modelBuilder.Entity("Trail365.Entities.Trail", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int?>("AltitudeAtStart")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("AnalyzerBlobID")
                        .HasColumnType("TEXT");

                    b.Property<int?>("AscentMeters")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AsphaltedRoadMeters")
                        .HasColumnType("INTEGER");

                    b.Property<string>("BoundingBox")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<int?>("DescentMeters")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<int?>("DistanceMeters")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("ElevationProfileImageID")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ElevationProfile_Advanced_ImageID")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ElevationProfile_Basic_ImageID")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ElevationProfile_Intermediate_ImageID")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ElevationProfile_Proficiency_ImageID")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("EndPlaceID")
                        .HasColumnType("TEXT");

                    b.Property<string>("Excerpt")
                        .HasColumnType("TEXT");

                    b.Property<string>("ExternalID")
                        .HasColumnType("TEXT");

                    b.Property<string>("ExternalSource")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("GpxBlobID")
                        .HasColumnType("TEXT");

                    b.Property<int>("GpxDownloadAccess")
                        .HasColumnName("GpxDownloadAccess")
                        .HasColumnType("INTEGER");

                    b.Property<string>("InternalDescription")
                        .HasColumnType("TEXT");

                    b.Property<int>("ListAccess")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("MaximumAltitude")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("MediumPreviewImageID")
                        .HasColumnType("TEXT");

                    b.Property<int?>("MinimumAltitude")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ModifiedByUser")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ModifiedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("OwnerID")
                        .HasColumnType("TEXT");

                    b.Property<int?>("PavedRoadMeters")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("PreviewImageID")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ScrapedUtc")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("SmallPreviewImageID")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("StartPlaceID")
                        .HasColumnType("TEXT");

                    b.Property<int?>("UnclassifiedMeters")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("UnpavedTrailMeters")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("AnalyzerBlobID");

                    b.HasIndex("ElevationProfileImageID");

                    b.HasIndex("EndPlaceID");

                    b.HasIndex("GpxBlobID");

                    b.HasIndex("MediumPreviewImageID");

                    b.HasIndex("PreviewImageID");

                    b.HasIndex("SmallPreviewImageID");

                    b.HasIndex("StartPlaceID");

                    b.ToTable("Trails");
                });

            modelBuilder.Entity("Trail365.Entities.User", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("GivenName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Surname")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserRoles")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Trail365.Entities.Event", b =>
                {
                    b.HasOne("Trail365.Entities.Blob", "CoverImage")
                        .WithMany()
                        .HasForeignKey("CoverImageID")
                        .HasConstraintName("FK_Event_CoverImage");

                    b.HasOne("Trail365.Entities.Place", "EndPlace")
                        .WithMany()
                        .HasForeignKey("EndPlaceID")
                        .HasConstraintName("FK_Event_EndPlace");

                    b.HasOne("Trail365.Entities.Place", "Place")
                        .WithMany()
                        .HasForeignKey("PlaceID")
                        .HasConstraintName("FK_Event_Place");

                    b.HasOne("Trail365.Entities.Trail", "Trail")
                        .WithMany("Events")
                        .HasForeignKey("TrailID")
                        .HasConstraintName("FK_Event_Trail");
                });

            modelBuilder.Entity("Trail365.Entities.EventInvolvement", b =>
                {
                    b.HasOne("Trail365.Entities.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Trail365.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .HasConstraintName("FK_EventInvolvement_User")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Trail365.Entities.FederatedIdentity", b =>
                {
                    b.HasOne("Trail365.Entities.User", "User")
                        .WithMany("Identities")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Trail365.Entities.Story", b =>
                {
                    b.HasOne("Trail365.Entities.Blob", "CoverImage")
                        .WithMany("StoryCovers")
                        .HasForeignKey("CoverImageID")
                        .HasConstraintName("FK_Story_CoverImage");

                    b.HasOne("Trail365.Entities.Event", null)
                        .WithMany("Stories")
                        .HasForeignKey("EventID");
                });

            modelBuilder.Entity("Trail365.Entities.StoryBlock", b =>
                {
                    b.HasOne("Trail365.Entities.Blob", "Image")
                        .WithMany("StoryBlocks")
                        .HasForeignKey("ImageID")
                        .HasConstraintName("FK_StoryBlock_Image");

                    b.HasOne("Trail365.Entities.Story", "Story")
                        .WithMany("StoryBlocks")
                        .HasForeignKey("StoryID")
                        .HasConstraintName("FK_StoryBlock_Story")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Trail365.Entities.Trail", b =>
                {
                    b.HasOne("Trail365.Entities.Blob", "AnalyzerBlob")
                        .WithMany()
                        .HasForeignKey("AnalyzerBlobID")
                        .HasConstraintName("FK_Trail_AnalyzerBlob");

                    b.HasOne("Trail365.Entities.Blob", "ElevationProfileImage")
                        .WithMany()
                        .HasForeignKey("ElevationProfileImageID")
                        .HasConstraintName("FK_Trail_ElevationProfileImage");

                    b.HasOne("Trail365.Entities.Place", "EndPlace")
                        .WithMany()
                        .HasForeignKey("EndPlaceID")
                        .HasConstraintName("FK_Trail_EndPlace");

                    b.HasOne("Trail365.Entities.Blob", "GpxBlob")
                        .WithMany()
                        .HasForeignKey("GpxBlobID")
                        .HasConstraintName("FK_Trail_GpxBlob");

                    b.HasOne("Trail365.Entities.Blob", "MediumPreviewImage")
                        .WithMany()
                        .HasForeignKey("MediumPreviewImageID")
                        .HasConstraintName("FK_Trail_MediumPreviewImage");

                    b.HasOne("Trail365.Entities.Blob", "PreviewImage")
                        .WithMany()
                        .HasForeignKey("PreviewImageID")
                        .HasConstraintName("FK_Trail_PreviewImage");

                    b.HasOne("Trail365.Entities.Blob", "SmallPreviewImage")
                        .WithMany()
                        .HasForeignKey("SmallPreviewImageID")
                        .HasConstraintName("FK_Trail_SmallPreviewImage");

                    b.HasOne("Trail365.Entities.Place", "StartPlace")
                        .WithMany()
                        .HasForeignKey("StartPlaceID")
                        .HasConstraintName("FK_Trail_StartPlace");
                });
#pragma warning restore 612, 618
        }
    }
}
