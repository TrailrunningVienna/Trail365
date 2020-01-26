using Microsoft.EntityFrameworkCore;
using Trail365.Entities;

namespace Trail365.Data
{
    public partial class TrailContext : DbContext, IDependencyTracker
    {
        /// <summary>
        /// SQLITE
        /// </summary>
        private static readonly string OperationType = "SQLITE";

        /// <summary>
        /// TrailContext
        /// </summary>
        private static readonly string OperationTarget = "TrailContext";

        string IDependencyTracker.OperationTarget => OperationTarget;

        string IDependencyTracker.OperationType(bool cached)
        {
            if (cached)
            {
                return "CACHE-" + OperationType;
            }
            else
            {
                return OperationType;
            }
        }

        public TrailContext(DbContextOptions<TrailContext> options) : base(options)
        {
        }

        public override int SaveChanges()
        {
            int? result = null;
            this.LogDependency(nameof(SaveChanges), (op) =>
            {
                result = base.SaveChanges();
                op.Properties.Add("StateEntriesChanged", result.Value.ToString());
            });
            return result.Value;
        }

        public DbSet<Trail> Trails { get; set; }

        public DbSet<Blob> Blobs { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<Place> Places { get; set; }

        public DbSet<Story> Stories { get; set; }

        public DbSet<StoryBlock> StoryBlocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blob>();

            modelBuilder.Entity<Place>();

            modelBuilder.Entity<Trail>()
                .HasOne(t => t.PreviewImage)
                .WithMany()
                .HasForeignKey(t => t.PreviewImageID)
                .HasConstraintName("FK_Trail_PreviewImage");

            modelBuilder.Entity<Trail>()
                .HasOne(t => t.MediumPreviewImage)
                .WithMany()
                .HasForeignKey(t => t.MediumPreviewImageID)
                .HasConstraintName("FK_Trail_MediumPreviewImage");

            modelBuilder.Entity<Trail>()
                .HasOne(t => t.SmallPreviewImage)
                .WithMany()
                .HasForeignKey(t => t.SmallPreviewImageID)
                .HasConstraintName("FK_Trail_SmallPreviewImage");

            modelBuilder.Entity<Trail>()
                .HasOne(t => t.ElevationProfileImage)
                .WithMany()
                .HasForeignKey(t => t.ElevationProfileImageID)
                .HasConstraintName("FK_Trail_ElevationProfileImage");

            modelBuilder.Entity<Trail>()
                .HasOne(t => t.StartPlace)
                .WithMany()
                .HasForeignKey(t => t.StartPlaceID)
                .HasConstraintName("FK_Trail_StartPlace");

            modelBuilder.Entity<Trail>()
                .HasOne(t => t.EndPlace)
                .WithMany()
                .HasForeignKey(t => t.EndPlaceID)
                .HasConstraintName("FK_Trail_EndPlace");

            modelBuilder.Entity<Trail>()
                .HasOne(t => t.GpxBlob)
                .WithMany()
                .HasForeignKey(t => t.GpxBlobID)
                .HasConstraintName("FK_Trail_GpxBlob");

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Place)
                .WithMany()
                .HasForeignKey(x => x.PlaceID)
                .HasConstraintName("FK_Event_Place");

            modelBuilder.Entity<Event>()
                .HasOne(e => e.EndPlace)
                .WithMany()
                .HasForeignKey(x => x.EndPlaceID)
                .HasConstraintName("FK_Event_EndPlace");

            modelBuilder.Entity<Event>()
                .HasOne(e => e.CoverImage)
                .WithMany()
                .HasForeignKey(x => x.CoverImageID)
                .HasConstraintName("FK_Event_CoverImage");

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Trail)
                .WithMany(t => t.Events)
                .HasForeignKey(x => x.TrailID)
                .HasConstraintName("FK_Event_Trail");

            modelBuilder.Entity<StoryBlock>()
               .HasOne(p => p.Story)
               .WithMany(st => st.StoryBlocks)
               .HasForeignKey(p => p.StoryID)
               .HasConstraintName("FK_StoryBlock_Story");

            modelBuilder.Entity<StoryBlock>()
               .HasOne(block => block.Image)
               .WithMany(i => i.StoryBlocks)
               .HasForeignKey(bl => bl.ImageID)
               .HasConstraintName("FK_StoryBlock_Image");

            modelBuilder.Entity<Story>()
               .HasOne(block => block.CoverImage)
               .WithMany(i => i.StoryCovers)
               .HasForeignKey(bl => bl.CoverImageID)
               .HasConstraintName("FK_Story_CoverImage");
        }
    }
}
