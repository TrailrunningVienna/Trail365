using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Internal;
using Trail365.Services;
using Trail365.ViewModels;

namespace Trail365.Data
{
    public static class TrailContextExtension
    {
        public static IQueryable<Event> OnlyPublished(this IQueryable<Event> input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            return input.Where(en => en.Status != EventStatus.Canceled && en.Status != EventStatus.Draft);
        }

        public static void AddEvent(this TrailContext context, BlobService blobService, IUrlHelper helper, params EventDto[] seedEvents)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));
            if (seedEvents == null) throw new ArgumentNullException(nameof(seedEvents));
            List<Event> createdEvents = new List<Event>();
            List<Place> createdPlaces = new List<Place>();
            List<Place> modifiedPlaces = new List<Place>();
            List<Blob> createdImages = new List<Blob>();
            foreach (var seedEvent in seedEvents)
            {
                Event e = seedEvent.ToEvent(); //Place not added/resolved!
                Place p = null;
                Blob coverImage = null;

                if ((seedEvent.Place != null) && (seedEvent.PlaceID.HasValue == false))
                {
                    p = context.EnsurePlace(seedEvent.Place, out var created, out var modified);
                    if (created)
                    {
                        createdPlaces.Add(p);
                    }
                    if (modified)
                    {
                        modifiedPlaces.Add(p);
                    }
                }

                if ((seedEvent.CoverImage != null) && (seedEvent.CoverImageID.HasValue == false))
                {
                    coverImage = context.Blobs.Find(seedEvent.CoverImage.ID);
                    if (coverImage == null)
                    {
                        var result = blobService.CreateBlobFromBlobDto(seedEvent.CoverImage, helper);
                        coverImage = result.Item1;
                        createdImages.Add(coverImage);
                    }
                    else
                    {
                        //ensure not changed content!
                    }
                }

                if (p != null)
                {
                    e.Place = p;
                    e.PlaceID = p.ID;
                }
                if (coverImage != null)
                {
                    e.CoverImage = coverImage;
                    e.CoverImageID = coverImage.ID;
                }
                createdEvents.Add(e);
            }
            context.Blobs.AddRange(createdImages);
            context.Places.AddRange(createdPlaces);
            context.Events.AddRange(createdEvents);
            context.Places.UpdateRange(modifiedPlaces);
        }

        public static Place GetPlaceCandidateOrDefault(IEnumerable<Place> source, PlaceDto place, bool ignorecity, string externalSourceOrDefault, string externalIDOrDefault)
        {
            //1. try to identify via FB-ID to cover renames
            if (!string.IsNullOrEmpty(externalIDOrDefault) && !string.IsNullOrEmpty(externalSourceOrDefault))
            {
                var q = source.Where(pl => pl.ExternalID == externalIDOrDefault && pl.ExternalSource == externalSourceOrDefault).FirstOrDefault();
                if (q != null)
                {
                    return q;
                }
            }

            var rawQuery = source.Where(pl => pl.Name == place.Name && pl.Country == place.Country);

            if (ignorecity == false)
            {
                rawQuery = rawQuery.Where(pl => pl.City == place.City);
            }

            var candidates = rawQuery.ToList();

            if (candidates.Count == 1)
            {
                return candidates.First();
            }
            return null;
        }

        public static Place EnsurePlace(this TrailContext context, PlaceDto place, out bool created, out bool modified)
        {
            return EnsurePlace(context, place, string.Empty, string.Empty, out created, out modified);
        }

        public static Place EnsurePlace(this TrailContext context, PlaceDto place, string externalSource, string externalID, out bool created, out bool modified)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (place == null) throw new ArgumentNullException(nameof(place));
            var item = context.Places.Find(place.ID);
            modified = false;
            if (item == null)
            {
                item = GetPlaceCandidateOrDefault(context.Places, place, false, externalSource, externalID); //ignorecity required in 11/2019 to convert some improvement on facebook!
                if (item == null)
                {
                    item = GetPlaceCandidateOrDefault(context.Places, place, true, externalSource, externalID); //ignorecity required in 11/2019 to convert some improvement on facebook!
                    if (item != null)
                    {
                        item.City = place.City;
                        item.Longitude = place.Longitude;
                        item.Latitude = place.Latitude;
                        item.ModifiedUtc = DateTime.UtcNow;

                        if (!string.IsNullOrEmpty(externalSource) && !string.IsNullOrEmpty(externalID))
                        {
                            if (string.IsNullOrEmpty(item.ExternalID) && string.IsNullOrEmpty(item.ExternalSource))
                            {
                                item.ExternalID = externalID;
                                item.ExternalSource = externalSource;
                            }
                        }
                        modified = true;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(externalSource) && !string.IsNullOrEmpty(externalID))
                    {
                        if (string.IsNullOrEmpty(item.ExternalID) && string.IsNullOrEmpty(item.ExternalSource))
                        {
                            item.ExternalID = externalID;
                            item.ExternalSource = externalSource;
                            item.ModifiedUtc = DateTime.UtcNow;
                            modified = true;
                        }
                    }
                }
            }

            if (item == null)
            {
                item = place.ToPlace();
                if (!string.IsNullOrEmpty(externalSource) && !string.IsNullOrEmpty(externalID))
                {
                    item.ExternalID = externalID;
                    item.ExternalSource = externalSource;
                }
                created = true;
            }
            else
            {
                created = false;
                //ensure not changed content!
            }

            Guard.AssertNotNull(item);
            return item;
        }

        public static int AddPlace(this TrailContext context, params PlaceDto[] seedPlaces)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (seedPlaces == null) return 0;
            List<Place> createdPlaces = new List<Place>();

            foreach (var item in seedPlaces)
            {
                Place p = context.EnsurePlace(item, out var created, out var modified);

                if (created == false)
                {
                    throw new InvalidOperationException("Duplicate Place");
                }
                createdPlaces.Add(p);
            }
            context.Places.AddRange(createdPlaces);
            return createdPlaces.Count;
        }

        /// <summary>
        /// </summary>
        /// <param name="uploadService"></param>
        /// <param name="files"></param>
        public static Tuple<GpxFileViewModel, Blob>[] AppendGpx(this BlobService uploadService, IFormFile[] files, IUrlHelper helper)
        {
            if (uploadService == null) throw new ArgumentNullException(nameof(uploadService));
            var result = new List<Tuple<GpxFileViewModel, Blob>>();
            if (files == null) return result.ToArray();

            foreach (var file in files)
            {
                if (file == null || file.Length <= 0) continue;

                var currentFile = new GpxFileViewModel() { ID = Guid.NewGuid(), Name = file.Name, FileName = file.FileName };

                using (var stream = file.OpenReadStream())
                {
                    Guard.Assert(stream.CanSeek);
                    string extension = Path.GetExtension(file.FileName);
                    string folderName = Utils.GetValidFolderName(extension);

                    if (folderName != "gpx")
                    {
                        throw new InvalidOperationException("Currently we allow only gpx files");
                    }

                    var blob = new Blob
                    {
                        ID = currentFile.ID
                    };
                    var mapping = uploadService.UploadStream(stream, currentFile.ID, folderName, extension, helper);
                    mapping.ApplyToBlob(blob);
                    blob.OriginalFileName = file.FileName;
                    Guard.Assert(mapping.Url.IsAbsoluteUri);
                    currentFile.AbsoluteUrl = mapping.Url.ToString();
                    result.Add(new Tuple<GpxFileViewModel, Blob>(currentFile, blob));
                }
            }
            return result.ToArray();
        }
    }
}
