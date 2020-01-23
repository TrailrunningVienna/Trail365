using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Internal;
using Trail365.Services;

namespace Trail365.Data
{
    public partial class TrailContext : DbContext
    {
        public bool TrailExists(Guid id)
        {
            using (var tracker = this.DependencyTracker(nameof(this.TrailExists)))
            {
                return this.Trails.Any(e => e.ID == id);
            }
        }

        public StoryBlock InsertStoryBlock(InsertMode mode, Guid storyID, Guid? relativeID)
        {
            var story = this.Stories.Find(storyID);
            StoryBlock relative = null;
            if (relativeID.HasValue)
            {
                relative = this.StoryBlocks.Find(relativeID.Value);
                if (relative == null)
                {
                    throw new InvalidOperationException("Not found");
                }
                if (relative.StoryID != storyID)
                {
                    throw new InvalidOperationException("story bug");
                }
            }
            var existingBlocks = this.StoryBlocks.Where(sb => sb.StoryID == storyID).OrderBy(sb => sb.SortOrder).ToList();
            int? newSort = null;
            switch (mode)
            {
                case InsertMode.First:
                    newSort = 0;
                    for (int i = 0; i < existingBlocks.Count; i++)
                    {
                        existingBlocks[i].SortOrder = i + 1;
                    }
                    break;

                case InsertMode.Last:
                    newSort = existingBlocks.Count;
                    for (int i = 0; i < existingBlocks.Count; i++)
                    {
                        existingBlocks[i].SortOrder = i;
                    }
                    break;

                case InsertMode.After:
                    int stepA = 0;
                    for (int i = 0; i < existingBlocks.Count; i++)
                    {
                        existingBlocks[i].SortOrder = i + stepA;
                        if (existingBlocks[i].ID == relativeID.Value)
                        {
                            newSort = i + 1;
                            stepA = 1;
                        }
                    }
                    break;

                case InsertMode.Before:
                    int stepB = 0;
                    for (int i = 0; i < existingBlocks.Count; i++)
                    {
                        if (existingBlocks[i].ID == relativeID.Value)
                        {
                            newSort = i + 1;
                            stepB = 1;
                        }
                        existingBlocks[i].SortOrder = i + stepB;
                    }
                    break;

                default:
                    throw new NotImplementedException("Mode-Enum");
            }

            StoryBlock workItem = new StoryBlock
            {
                StoryID = storyID,
                SortOrder = newSort.Value
            };
            this.StoryBlocks.Add(workItem);
            return workItem;
        }

        public void SeedTrails(BlobService blobService, TrailDto[] trails, IUrlHelper helper)
        {
            TrailContext context = this;
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (blobService == null) throw new ArgumentNullException(nameof(context));
            if (helper == null) throw new ArgumentNullException(nameof(helper));

            List<Trail> trailBatch = new List<Trail>();
            List<Blob> blobBatch = new List<Blob>();
            foreach (var seedTrail in trails)
            {
                Trail entityTrail = new Trail
                {
                    ID = seedTrail.ID,
                    Description = seedTrail.Description,
                    InternalDescription = seedTrail.InternalDescription,
                    GpxDownloadAccess = seedTrail.GpxDownloadAccess,
                    ListAccess = seedTrail.ListAccess,
                    Excerpt = seedTrail.Excerpt,
                };

                string hashcode = HashUtils.CalculateHash(seedTrail.Gpx);

                TrailExtender.ReadGpxFileInfo(seedTrail.Gpx, entityTrail);

                if (string.IsNullOrEmpty(seedTrail.Name) == false)
                {
                    entityTrail.Name = seedTrail.Name; // if there is a name on the input then it wins!
                }
                entityTrail.GpxBlob = new Blob();
                entityTrail.GpxBlobID = entityTrail.GpxBlob.ID;
                blobService.UploadBytesAsGpx(seedTrail.Gpx, entityTrail.GpxBlob, helper);
                entityTrail.GpxBlob.ContentHash = hashcode;
                trailBatch.Add(entityTrail);
                blobBatch.Add(entityTrail.GpxBlob);
                Guard.Assert(!string.IsNullOrEmpty(entityTrail.GpxBlob.FolderName));
            }
            context.Trails.AddRange(trailBatch);
            context.Blobs.AddRange(blobBatch);
        }

        public List<Tuple<string, string>> GetTrailReferences(Guid id, IUrlHelper helper)
        {
            using (var operation = this.DependencyTracker(nameof(this.GetTrailReferences)))
            {
                var usageOnEvents = this.Events.Where(e => e.TrailID == id).Select(e => new Tuple<string, string>(helper.GetEventUrl(e.ID, true, false), e.Name)).ToList();
                var usageOnStories = new List<Tuple<string, string>>();
                return usageOnEvents.Concat(usageOnStories).ToList();
            }
        }

        public List<Tuple<string, string>> GetPlaceReferences(Guid id, IUrlHelper helper)
        {
            using (var operation = this.DependencyTracker(nameof(this.GetPlaceReferences)))
            {
                var usageOnEvents = this.Events.Where(e => e.PlaceID == id).Select(e => new Tuple<string, string>(helper.GetEventUrl(e.ID, true, false), e.GetReferenceCaption())).ToList();
                var usageOnTrails = this.Trails.Where(t => t.StartPlaceID == id || t.EndPlaceID == id).Distinct().Select(t => new Tuple<string, string>(helper.GetTrailDetailsUrl(t.ID, true, false), t.Name)).ToList();
                return usageOnEvents.Concat(usageOnTrails).ToList();
            }
        }

        public void ImportTrails(IEnumerable<Trail> trails)
        {
            if (trails == null) throw new ArgumentNullException(nameof(trails));

            List<Trail> created = new List<Trail>();
            List<Trail> modified = new List<Trail>();
            List<Blob> createdBlobs = new List<Blob>();
            List<Blob> modifiedBlobs = new List<Blob>();

            foreach (var trail in trails)
            {
                var existing = this.Trails.SingleOrDefault(t => t.ID == trail.ID);
                if (existing == null)
                {
                    created.Add(trail);
                    createdBlobs.Add(trail.GpxBlob);
                }
                else
                {
                    //WM 08.08.2019 DATA Overwrite!
                    //existing.CopyFrom(trail);
                    //modified.Add(existing);
                }
            }
            this.Trails.AddRange(created);
            this.Trails.UpdateRange(modified);
            this.Blobs.AddRange(createdBlobs);
            this.Blobs.UpdateRange(modifiedBlobs);
            this.SaveChanges();
        }

        public void DeleteBlob(Blob blob, BlobService blobService)
        {
            //TODO Missing reference check, but currently we have so many references....
            if (blob == null) throw new ArgumentNullException(nameof(blob));
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));
            blobService.Remove(blob.ID, blob.FolderName, blob.Url);
            this.Blobs.Remove(blob);
        }

        public void DeleteStoryBlock(StoryBlock model, BlobService blobService)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));
            if (model.Image != null)
            {
                this.DeleteBlob(model.Image, blobService);
            }
            this.Remove(model);
        }

        public void DeleteStory(Story model, BlobService blobService)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));
            var blocks = this.StoryBlocks.Include(sb => sb.Image).Where(sb => sb.StoryID == model.ID).ToList();
            blocks.ForEach(bl =>
            {
                this.DeleteStoryBlock(bl, blobService);
            });

            this.Remove(model);
        }

        public void DeletePlace(Place entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var references = this.GetPlaceReferences(entity.ID, HelperExtensions.EmptyUrlHelper);
            if (references.Count > 0)
            {
                throw new InvalidOperationException($"Place '{entity.Name}' can't be deleted because it has references");
            }
            this.Remove(entity);
        }

        public Blob GetBlobByID(Guid id)
        {
            Blob result = null;
            this.LogDependency(nameof(this.GetBlobByID), t =>
            {
                result = this.Blobs.Find(id);
            });
            if (result == null)
            {
                throw new InvalidOperationException($"Blob with ID='{id}' not found");
            }
            return result;
        }

        public void DeleteEvent(Event entity, BlobService blobService)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));
            this.Remove(entity);
        }

        public void DeleteTrail(Trail trail, BlobService blobService)
        {
            if (trail == null) throw new ArgumentNullException(nameof(trail));
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));

            var references = this.GetTrailReferences(trail.ID, HelperExtensions.EmptyUrlHelper);

            if (references.Count > 0)
            {
                throw new InvalidOperationException($"Trail '{trail.Name}' can't be deleted because it has references");
            }

            if (trail.GpxBlobID.HasValue)
            {
                var gpx = this.GetBlobByID(trail.GpxBlobID.Value);
                this.DeleteBlob(gpx, blobService);
            }

            if (trail.PreviewImageID.HasValue)
            {
                var img = this.GetBlobByID(trail.PreviewImageID.Value);
                this.DeleteBlob(img, blobService);
            }

            if (trail.SmallPreviewImageID.HasValue)
            {
                var img = this.GetBlobByID(trail.SmallPreviewImageID.Value);
                this.DeleteBlob(img, blobService);
            }

            if (trail.MediumPreviewImageID.HasValue)
            {
                var img = this.GetBlobByID(trail.MediumPreviewImageID.Value);
                this.DeleteBlob(img, blobService);
            }

            if (trail.ElevationProfileImageID.HasValue)
            {
                var img = this.GetBlobByID(trail.ElevationProfileImageID.Value);
                this.DeleteBlob(img, blobService);
            }

            if (trail.ElevationProfile_Basic_ImageID.HasValue)
            {
                var img = this.GetBlobByID(trail.ElevationProfile_Basic_ImageID.Value);
                this.DeleteBlob(img, blobService);
            }

            if (trail.ElevationProfile_Intermediate_ImageID.HasValue)
            {
                var img = this.GetBlobByID(trail.ElevationProfile_Intermediate_ImageID.Value);
                this.DeleteBlob(img, blobService);
            }

            if (trail.ElevationProfile_Advanced_ImageID.HasValue)
            {
                var img = this.GetBlobByID(trail.ElevationProfile_Advanced_ImageID.Value);
                this.DeleteBlob(img, blobService);
            }

            if (trail.ElevationProfile_Proficiency_ImageID.HasValue)
            {
                var img = this.GetBlobByID(trail.ElevationProfile_Proficiency_ImageID.Value);
                this.DeleteBlob(img, blobService);
            }

            this.Remove(trail);
        }
    }
}
