using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trail365.Data;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Internal;
using Trail365.Services;

namespace Trail365.Services
{
    public class FacebookEventImporter : EventImporter<FacebookEventData>
    {
        public Place EnsurePlace(PlaceDto place, string externalSource, string externalID)
        {
            if (place == null) throw new ArgumentNullException(nameof(place));

            //Two Level EnsurePlace: Level1 for Inserted Places, Level 2 for Places inside DB!
            var item = TrailContextExtension.GetPlaceCandidateOrDefault(this.InsertedPlaces.Concat(this.UpdatedPlaces), place, false, externalSource, externalID);

            if (item == null)
            {
                item = this.DbContext.EnsurePlace(place, externalSource, externalID, out var placeCreated, out var placeModified);

                Guard.AssertNotNull(item);

                if (placeCreated)
                {
                    this.InsertedPlaces.Add(item);
                }

                if (placeModified)
                {
                    item.ModifiedUtc = this.UtcNow;
                    this.UpdatedPlaces.Add(item);
                }
            }
            return item;
        }

        private readonly DownloadService DownloadService;
        private readonly BlobService BlobService;

        public FacebookEventImporter(TrailContext context, IUrlHelper helper, DownloadService downloadService, BlobService blobService) : base(context, helper)
        {
            this.DownloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
            this.BlobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
        }

        public override Task Import(FacebookEventData data, ILogger logger, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            try
            {
                var events = this.DbContext.Events.Include(e => e.CoverImage).Include(e => e.Place).Where(item => item.ExternalSource == data.ExternalSource).ToList();
                logger.LogInformation($"{data.Events.Length} events retrieved from Facebook API (ExternalSource={data.ExternalSource})");
                logger.LogInformation($"{events.Count} events found inside database for externalSource={data.ExternalSource})");

                try
                {
                    foreach (var fbSeed in data.Events)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var exists = events.SingleOrDefault(t => t.ExternalSource == data.ExternalSource && t.ExternalID == fbSeed.Id);

                        if (fbSeed.IsDraft)
                        {
                            if (exists != null)
                            {
                                logger.LogWarning($"Drafts for existing items not implemented ({fbSeed.Name})");
                                continue;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        logger.LogTrace($"Event='{fbSeed.Name}', exists={exists != null}");
                        Event target = null;
                        bool isCreation;
                        bool isModified = false;

                        if (exists != null)
                        {
                            target = exists;
                            isCreation = false;
                        }
                        else
                        {
                            target = new Event
                            {
                                CreatedUtc = this.UtcNow,
                                Name = fbSeed.Name,
                                ExternalSource = data.ExternalSource,
                                ExternalID = fbSeed.Id
                            };

                            this.InsertedEvents.Add(target);
                            isCreation = true;
                        }

                        bool hasDescriptionLock = ((target.DescriptionLock & ContentLock.ImportLock) == ContentLock.ImportLock);
                        if (hasDescriptionLock)
                        {
                            logger.LogDebug($"DescriptionLock found for Event '{fbSeed.Name}'");
                        }

                        if (target.Name != fbSeed.Name) isModified = true;
                        if (target.StartTimeUtc != fbSeed.StartTime.UtcDateTime) isModified = true;

                        if (!hasDescriptionLock)
                        {
                            if (target.Description != fbSeed.Description) isModified = true;
                        }

                        if (target.EndTimeUtc != fbSeed.EndTime.UtcDateTime) isModified = true;

                        EventStatus seedStatus = EventStatus.Default;
                        if (fbSeed.IsCanceled)
                        {
                            seedStatus = EventStatus.Canceled;
                            logger.LogDebug($"Event-Cancellation found for Event '{fbSeed.Name}'");
                        }

                        if (fbSeed.IsDraft)
                        {
                            seedStatus = EventStatus.Draft;
                        }

                        if (target.Status != seedStatus)
                        {
                            isModified = true;
                        }

                        target.Name = fbSeed.Name;
                        target.StartTimeUtc = fbSeed.StartTime.UtcDateTime;
                        target.EndTimeUtc = fbSeed.EndTime.UtcDateTime;

                        if (!hasDescriptionLock)
                        {
                            target.Description = fbSeed.Description;
                        }

                        target.Status = seedStatus;

                        if (fbSeed.Cover != null)
                        {
                            if (isCreation || target.CoverImageID.HasValue == false)
                            {
                                BlobDto extractedCover = this.DownloadService.DownloadFromUrl(fbSeed.Cover.Source);
                                Guard.Assert(extractedCover.Url == null, "not initialized because not inside our storage until now!");

                                var resultTuple = this.BlobService.CreateBlobFromBlobDto(extractedCover, this.Url);
                                target.CoverImage = resultTuple.Item1;
                                target.CoverImageID = target.CoverImage.ID;
                                this.InsertedImages.Add(target.CoverImage);

                                if (isCreation == false)
                                {
                                    isModified = true;
                                }
                            }
                            else
                            {
                            }
                        }

                        if (fbSeed.Place != null)
                        {
                            PlaceDto dto = new PlaceDto

                            {
                                Name = fbSeed.Place.Name
                            };

                            if (fbSeed.Place.Location != null)
                            {
                                dto.Country = fbSeed.Place.Location.Country;
                                dto.Latitude = fbSeed.Place.Location.Latitude;
                                dto.Longitude = fbSeed.Place.Location.Longitude;
                                dto.City = fbSeed.Place.Location.City;
                            }

                            if (target.PlaceLock != ContentLock.ImportLock)
                            {
                                var placeEntity = this.EnsurePlace(dto, data.ExternalSource, dto.GetExternalID());
                                target.Place = placeEntity;
                                target.PlaceID = placeEntity.ID;
                            }
                        }
                        else
                        {
                            if (target.PlaceID.HasValue)
                            {
                                throw new NotImplementedException("remove place reference");
                            }
                        }

                        if (isCreation == false)
                        {
                            target.CreatedUtc = exists.CreatedUtc;
                        }

                        if ((isCreation == false) && (isModified == true))
                        {
                            target.ModifiedUtc = this.UtcNow;
                            this.UpdatedEvents.Add(target);
                        }

                        if (fbSeed.StartTime < DateTimeOffset.Now.AddDays(-3))
                        {
                            //use the event-starttime in the past as creation date, because we don't know the exact creation date!
                            target.CreatedUtc = fbSeed.StartTime.UtcDateTime;
                        }

                        target.ListAccess = AccessLevel.Public;
                    }
                }
                finally
                {
                    this.DbContext.Places.AddRange(this.InsertedPlaces);
                    this.DbContext.Blobs.AddRange(this.InsertedImages);
                    this.DbContext.Events.AddRange(this.InsertedEvents);
                    this.DbContext.Events.UpdateRange(this.UpdatedEvents);
                    this.DbContext.Places.UpdateRange(this.UpdatedPlaces);
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception during FB-Import");
                throw;
            }
        }
    }
}
