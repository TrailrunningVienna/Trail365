using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Trail365.Entities;
using Trail365.Internal;

namespace Trail365.Data
{
    public partial class TrailContext : DbContext
    {
        public List<Story> GetStoriesByFilter(StoryQueryFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            IMemoryCache cache = filter.Cache;
            if (filter.AbsoluteExpiration == TimeSpan.Zero)
            {
                cache = null;
            }

            string fullKey = null;

            if (cache != null)
            {
                fullKey = filter.GetCacheKey();
                var sw = Stopwatch.StartNew();

                var hit = cache.TryGetValue<List<Story>>(fullKey, out var cacheResults);
                sw.Stop();
                if (hit)
                {
                    this.LogCacheDependency(nameof(GetStoriesByFilter), t =>
                    {
                        t.Duration = sw.Elapsed;
                        t.Data = fullKey;
                        t.Metrics.Add("Results", cacheResults.Count);
                    });
                    return cacheResults;
                }
            }

            using (var operation = this.DependencyTracker(nameof(this.GetStoriesByFilter)))
            {
                operation.Telemetry.Data = filter.GetCommandText();
                var query = this.GetStoriesByFilterQueryable(filter);
                var result = query.ToList();
                operation.Telemetry.Metrics.Add("Results", result.Count);

                if (cache != null)
                {
                    cache.Set(fullKey, result, filter.AbsoluteExpiration);
                }
                return result;
            }
        }

        public bool HasAnyStory(StoryQueryFilter filter)
        {
            using (var operation = this.DependencyTracker(nameof(this.HasAnyStory)))
            {
                operation.Telemetry.Data = filter.GetCommandText();
                var query = this.GetStoriesByFilterQueryable(filter);
                var result = query.Any(); //Any(), executed directly on the IQueryable should result in faster SQL.
                return result;
            }
        }

        public int GetStoryCount(StoryQueryFilter filter)
        {
            using (var operation = this.DependencyTracker(nameof(this.GetStoryCount)))
            {
                operation.Telemetry.Data = filter.GetCommandText();
                var query = this.GetStoriesByFilterQueryable(filter);
                var result = query.Count(); //Count, executed directly on the IQueryable should result in faster SQL.
                operation.Telemetry.Metrics.Add("Results", result);
                return result;
            }
        }

        public IQueryable<Story> GetStoriesByFilterQueryable(StoryQueryFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            if ((filter.Take.HasValue) && (filter.OrderBy == StoryQueryOrdering.None))
            {
                throw new InvalidOperationException("TopN requires OrderBy to prevent random results");
            }

            IQueryable<Story> baseQuery = this.Stories;

            if (filter.IncludeBlocks)
            {
                baseQuery = baseQuery.Include(s => s.StoryBlocks).ThenInclude(sb => sb.Image);
                baseQuery = baseQuery.Include(s => s.CoverImage);
            }

            if (filter.FilterByAllowedLevels)
            {
                baseQuery = baseQuery.Where(t => filter.AllowedLevels.Contains(t.ListAccess));
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
                foreach (var searchtextpart in filter.SearchText.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    baseQuery = baseQuery.Where(e => e.Name.ToLower().Contains(searchtextpart));
                }

            if (filter.StoryID.HasValue)
            {
                baseQuery = baseQuery.Where(e => e.ID == filter.StoryID.Value);
            }

            if (filter.ExcludedStatus != null && filter.ExcludedStatus.Length > 0)
            {
                baseQuery = baseQuery.Where(e => !filter.ExcludedStatus.Contains(e.Status));
            }

            if (filter.IncludedStatus != null && filter.IncludedStatus.Length > 0)
            {
                baseQuery = baseQuery.Where(e => filter.IncludedStatus.Contains(e.Status));
            }

            switch (filter.OrderBy)
            {
                case StoryQueryOrdering.None:
                    break;

                case StoryQueryOrdering.DescendingCreationOrModificationDate:
                    baseQuery = baseQuery.OrderByDescending(item => item.ModifiedUtc ?? item.CreatedUtc);
                    break;

                case StoryQueryOrdering.AscendingName:
                    baseQuery = baseQuery.OrderBy(s => s.Name);
                    break;

                case StoryQueryOrdering.DescendingName:
                    baseQuery = baseQuery.OrderByDescending(s => s.Name);
                    break;

                default:
                    throw new NotSupportedException($"{nameof(EventQueryOrdering)} => {filter.OrderBy}");
            }

            if (filter.Take.HasValue)
            {
                baseQuery = baseQuery.Take(filter.Take.Value);
            }
            if (filter.Skip.HasValue)
            {
                baseQuery = baseQuery.Skip(filter.Skip.Value);
            }
            return baseQuery;
        }

        public List<Event> GetEventsByFilter(EventQueryFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            if ((filter.TopN.HasValue) && (filter.OrderBy == EventQueryOrdering.None))
            {
                throw new InvalidOperationException("TopN requires OrderBy to prevent random results");
            }

            using (var operation = this.DependencyTracker(nameof(this.GetEventsByFilter)))
            {
                operation.Telemetry.Target = OperationTarget;
                operation.Telemetry.Type = OperationType;

                IQueryable<Event> baseQuery = this.Events;

                if (filter.IncludePlaces)
                {
                    baseQuery = baseQuery.Include(s => s.Place);
                }

                if (filter.IncludeImages)
                {
                    baseQuery = baseQuery.Include(s => s.CoverImage);
                }

                if (filter.IncludeTrails)
                {
                    baseQuery = baseQuery.Include(s => s.Trail).ThenInclude(t => t.GpxBlob);
                }

                if (filter.FilterByAllowedLevels)
                {
                    baseQuery = baseQuery.Where(t => filter.AllowedLevels.Contains(t.ListAccess));
                }

                if (filter.StartTimeUtcMinValue.HasValue)
                {
                    baseQuery = baseQuery.Where(e => e.StartTimeUtc >= filter.StartTimeUtcMinValue);
                }

                if (filter.EventID.HasValue)
                {
                    baseQuery = baseQuery.Where(e => e.ID == filter.EventID.Value);
                }

                if (filter.ExcludedStatus != null && filter.ExcludedStatus.Length > 0)
                {
                    baseQuery = baseQuery.Where(e => !filter.ExcludedStatus.Contains(e.Status));
                }

                if (filter.OwnerID.HasValue)
                {
                    baseQuery = baseQuery.Where(e => e.OwnerID == filter.OwnerID.Value);
                }

                if (filter.ExcludedEvents.Count > 0)
                {
                    baseQuery = baseQuery.Where(e => !filter.ExcludedEvents.Contains(e.ID));
                }

                switch (filter.OrderBy)
                {
                    case EventQueryOrdering.None:
                        break;

                    case EventQueryOrdering.DescendingCreationOrModificationDate:
                        baseQuery = baseQuery.OrderByDescending(item => item.ModifiedUtc ?? item.CreatedUtc);
                        break;

                    case EventQueryOrdering.AscendingStartDate:
                        baseQuery = baseQuery.OrderBy(en => en.StartTimeUtc);
                        break;

                    case EventQueryOrdering.AscendingName:
                        baseQuery = baseQuery.OrderBy(en => en.Name);
                        break;

                    default:
                        throw new NotSupportedException($"{nameof(EventQueryOrdering)} => {filter.OrderBy}");
                }

                if (filter.TopN.HasValue)
                {
                    baseQuery = baseQuery.Take(filter.TopN.Value);
                }

                operation.Telemetry.Data = filter.GetCommandText();

                var result = baseQuery.ToList();

                if (filter.IncludePlaces)
                {
                    //migration for foreign key not implemented (SQLite restriction)
                    result.Where(e => e.EndPlaceID.HasValue).ToList().ForEach(e =>
                      {
                          e.EndPlace = this.Places.Find(e.EndPlaceID.Value);
                      });
                }

                if (filter.ExcludedEvents.Count > 0)
                {
                    //WM 23.12.2019 "Contains" seems not working not working serverside!
                    result = result.Where(e => !filter.ExcludedEvents.Contains(e.ID)).ToList();
                }
                return result;
            }
        }

        public List<Event> GetEvents(bool inlcudePlaces, bool includeImages, bool includeTrail, int? topN = null)
        {
            EventQueryFilter qf = new EventQueryFilter()
            {
                IncludeImages = includeImages,
                IncludeTrails = includeTrail,
                IncludePlaces = inlcudePlaces,
                TopN = topN
            };
            return this.GetEventsByFilter(qf);
        }

        public List<Tuple<Event, int>> GetEventStreamForNews(int maxToPromote, int maxFromHistory, DateTime utcNow, params AccessLevel[] allowedLevels)
        {
            return this.GetEventStreamForNews(maxToPromote, maxFromHistory, utcNow, null, allowedLevels, 0);
        }

        public List<Tuple<Event, int>> GetEventStreamForNews(int maxToPromote, int maxFromHistory, DateTime utcNow, IMemoryCache cache, AccessLevel[] allowedLevels, int CacheDurationSeconds)
        {
            Guard.Assert(utcNow.Kind == DateTimeKind.Utc);
            Guard.Assert(utcNow.Kind == DateTimeKind.Utc);

            List<Tuple<Event, int>> results = null;

            var levelArgs = allowedLevels.OrderBy(l => l).Select(l => l.ToString().ToLowerInvariant());
            var coreArgs = new string[] { maxToPromote.ToString(), maxFromHistory.ToString() };
            var fullKey = string.Join('|', coreArgs.Concat(levelArgs));

            if (cache != null)
            {
                var sw = Stopwatch.StartNew();
                var hit = cache.TryGetValue<List<Tuple<Event, int>>>(fullKey, out results);
                sw.Stop();
                if (hit)
                {
                    this.LogCacheDependency(nameof(GetEventStreamForNews), t =>
                    {
                        t.Duration = sw.Elapsed;
                        t.Data = fullKey;
                        t.Metrics.Add("Results", results.Count);
                    });
                    return results;
                }
            }

            this.LogDependency(nameof(GetEventStreamForNews), t =>
            {
                t.Data = fullKey;

                bool includePlaces = true; //we need places for Metadata!
                bool includeImages = true;
                bool includeTrails = true; //we need trails for Metadata!

                List<Event> eventsToPromote;

                if (maxToPromote > 0)
                {
                    EventQueryFilter filter = new EventQueryFilter(allowedLevels, restrictToPublishedEventsOnly: true)
                    {
                        TopN = maxToPromote,
                        IncludeImages = includeImages,
                        IncludePlaces = includePlaces,
                        IncludeTrails = includeTrails,
                        OrderBy = EventQueryOrdering.AscendingStartDate
                    };
                    filter.StartTimeUtcMinValue = utcNow;
                    eventsToPromote = this.GetEventsByFilter(filter);
                    var check21 = eventsToPromote.Where(en => en.StartTimeUtc < utcNow).ToList();
                    Guard.Assert(check21.Count == 0);
                }
                else
                {
                    eventsToPromote = new List<Event>();
                }

                var IdsToPromote = eventsToPromote.Select(ev => ev.ID).ToList();

                results = new List<Tuple<Event, int>>();

                int eventsBasePriority = 1000; //to distinguish from stories, tracks, places etc

                var converted = eventsToPromote.Select(e =>
                {
                    int index = eventsToPromote.IndexOf(e);
                    int prio = eventsBasePriority + (maxToPromote - index);
                    return new Tuple<Event, int>(e, prio);
                }).ToList();

                results.AddRange(converted);

                if (maxFromHistory > 0)
                {
                    EventQueryFilter filter = new EventQueryFilter(allowedLevels, restrictToPublishedEventsOnly: true)
                    {
                        IncludeImages = includeImages,
                        IncludePlaces = includePlaces,
                        IncludeTrails = includeTrails,
                        OrderBy = EventQueryOrdering.DescendingCreationOrModificationDate,
                        TopN = maxFromHistory,
                    };

                    filter.ExcludedEvents.AddRange(IdsToPromote);

                    var eventsByMetadata = this.GetEventsByFilter(filter);

                    converted = eventsByMetadata.Select(e => new Tuple<Event, int>(e, 0)).ToList();

                    results.AddRange(converted);
                }
                Guard.Assert(eventsToPromote.Count == eventsToPromote.Select(e1 => e1.ID).Distinct().Count(), "distinct alarm");
                t.Metrics.Add("Results", results.Count);
            });

            if ((cache != null) && (CacheDurationSeconds > 0))
            {
                cache.Set(fullKey, results, TimeSpan.FromSeconds(CacheDurationSeconds));
            }

            return results;
        }

        public List<Place> GetPlacesForSelectList()
        {
            List<Place> result = null;
            this.LogDependency(nameof(this.GetPlacesForSelectList), t =>
            {
                result = this.Places.OrderBy(p => p.Name).ToList();
            });
            Guard.AssertNotNull(result);
            return result;
        }

        public List<Event> GetEventsByListAccess(bool includePlaces, bool includeImages, bool includeTrails, int? topN, AccessLevel[] allowedLevels)
        {
            EventQueryFilter filter = new EventQueryFilter(allowedLevels, restrictToPublishedEventsOnly: true)
            {
                IncludeImages = includeImages,
                IncludePlaces = includePlaces,
                IncludeTrails = includeTrails,
                TopN = topN
            };
            return this.GetEventsByFilter(filter);
        }

        public IQueryable<Story> GetStories(AccessLevel[] allowedAccessLevels, bool inlcudeImages)
        {
            var filter = new StoryQueryFilter(allowedAccessLevels, false)
            {
                IncludeBlocks = inlcudeImages
            };
            var baseQuery = this.GetStoriesByFilterQueryable(filter);
            return baseQuery;
        }

        public List<Trail> GetTrailsByFilter(TrailQueryFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            using (var operation = this.DependencyTracker(nameof(this.GetTrailsByFilter)))
            {
                operation.Telemetry.Target = OperationTarget;
                operation.Telemetry.Type = OperationType;

                IQueryable<Trail> baseQuery = this.Trails;

                if (filter.TrailID.HasValue)
                {
                    baseQuery = baseQuery.Where(t => t.ID == filter.TrailID.Value);
                }

                if (filter.IncludeGpxBlob)
                {
                    baseQuery = baseQuery.Include(t => t.GpxBlob);
                }
                if (filter.IncludeImages)
                {
                    baseQuery = baseQuery.Include(t => t.SmallPreviewImage);
                    baseQuery = baseQuery.Include(t => t.ElevationProfileImage);
                }

                if (filter.IncludePlaces)
                {
                    baseQuery = baseQuery.Include(t => t.StartPlace);
                    baseQuery = baseQuery.Include(t => t.EndPlace);
                }
                if (filter.HasSearchText())
                {
                    foreach (var split in filter.GetSearchTextSplitsInLowerCase())
                    {
                        //ATTENTION AND and OR implicites
                        baseQuery = baseQuery.Where(t => EF.Functions.Like(t.Name, "%" + split + "%") || EF.Functions.Like(t.Excerpt, "%" + split + "%")); //WM 08/2019 we use only name and excerpt for search
                    }
                }

                if (filter.OwnerID.HasValue)
                {
                    baseQuery = baseQuery.Where(t => t.OwnerID == filter.OwnerID.Value);
                }

                if (filter.FilterByAllowedLevels)
                {
                    baseQuery = baseQuery.Where(t => filter.AllowedLevels.Contains(t.ListAccess));
                }

                switch (filter.OrderBy)
                {
                    case TrailQueryOrdering.None:
                        break;

                    case TrailQueryOrdering.DescendingCreationOrModificationDate:
                        baseQuery = baseQuery.OrderByDescending(item => item.ModifiedUtc ?? item.CreatedUtc);
                        break;

                    case TrailQueryOrdering.AscendingName:
                        baseQuery = baseQuery.OrderBy(en => en.Name);
                        break;

                    default:
                        throw new NotSupportedException($"{nameof(TrailQueryOrdering)} => {filter.OrderBy}");
                }

                if (filter.Take.HasValue)
                {
                    baseQuery = baseQuery.Take(filter.Take.Value);
                }

                if (filter.Skip.HasValue)
                {
                    baseQuery = baseQuery.Skip(filter.Skip.Value);
                }

                var result = baseQuery.ToList();
                operation.Telemetry.Data = filter.GetCommandText();
                return result;
            }
        }

        public List<Trail> GetTrails(bool includePlaces, bool includeImages)
        {
            TrailQueryFilter f = new TrailQueryFilter()
            {
                IncludePlaces = includePlaces,
                IncludeImages = includeImages,
            };
            return this.GetTrailsByFilter(f);
        }

        public List<Trail> GetTrailsByListAccessFilter(params AccessLevel[] allowedLevels)
        {
            TrailQueryFilter filter = new TrailQueryFilter(allowedLevels)
            {
                IncludeImages = false,
                IncludePlaces = false,
            };
            return this.GetTrailsByFilter(filter);
        }

        public List<Trail> GetTrailsByListAccessOrderByDateDescending(bool includeImages, bool includeGpxBlob, AccessLevel[] allowedLevels, int topN, IMemoryCache cache, int cacheDurationSeconds)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            return this.InternalGetTrailsByListAccessOrderByDateDescending(includeImages, includeGpxBlob, allowedLevels, topN, cache, cacheDurationSeconds);
        }

        public List<Trail> GetTrailsByListAccessOrderByDateDescending(bool includeImages, bool includeGpxBlob, AccessLevel[] allowedLevels, int topN)
        {
            return this.InternalGetTrailsByListAccessOrderByDateDescending(includeImages, includeGpxBlob, allowedLevels, topN, null, 0);
        }

        internal List<Trail> InternalGetTrailsByListAccessOrderByDateDescending(bool includeImages, bool includeGpxBlob, AccessLevel[] allowedLevels, int topN, IMemoryCache cache, int cacheDurationSeconds)
        {
            var levelArgs = allowedLevels.OrderBy(l => l).Select(l => l.ToString().ToLowerInvariant());
            var coreArgs = new string[] { includeImages.ToString(), includeGpxBlob.ToString(), topN.ToString() };
            var fullKey = string.Join('|', coreArgs.Concat(levelArgs));

            if (cache != null)
            {
                var sw = Stopwatch.StartNew();
                var hit = cache.TryGetValue<List<Trail>>(fullKey, out var resultsFromCache);
                sw.Stop();
                if (hit)
                {
                    this.LogCacheDependency(nameof(GetTrailsByListAccessOrderByDateDescending), t =>
                    {
                        t.Duration = sw.Elapsed;
                        t.Data = fullKey;
                        t.Metrics.Add("Results", resultsFromCache.Count);
                    });
                    return resultsFromCache;
                }
            }
            List<Trail> results = null;

            this.LogDependency(nameof(GetTrailsByListAccessOrderByDateDescending), t =>
            {
                t.Data = fullKey;
                TrailQueryFilter qf = new TrailQueryFilter(allowedLevels)
                {
                    Take = topN,
                    IncludeImages = includeImages,
                    IncludePlaces = false,
                    OrderBy = TrailQueryOrdering.DescendingCreationOrModificationDate,
                    IncludeGpxBlob = includeGpxBlob
                };
                results = this.GetTrailsByFilter(qf);
                t.Metrics.Add("Results", results.Count);
            });

            if ((cache != null) && (cacheDurationSeconds > 0))
            {
                cache.Set(fullKey, results, TimeSpan.FromSeconds(cacheDurationSeconds));
            }

            return results;
        }

        public Blob[] GetRelatedPreviewImages(params Trail[] trailList)
        {
            var imagesIDListFullPreview = trailList.Where(t => t.PreviewImageID != null).Select(t => t.PreviewImageID.Value).ToList();
            var imagesIDListSmallPreview = trailList.Where(t => t.SmallPreviewImageID != null).Select(t => t.SmallPreviewImageID.Value).ToList();
            var imagesIDListMediumPreview = trailList.Where(t => t.MediumPreviewImageID != null).Select(t => t.MediumPreviewImageID.Value).ToList();
            var elevationProfileIDList = trailList.Where(t => t.ElevationProfileImageID != null).Select(t => t.ElevationProfileImageID.Value).ToList();

            var basic = trailList.Where(t => t.ElevationProfile_Basic_ImageID != null).Select(t => t.ElevationProfile_Basic_ImageID.Value).ToList();
            var intermediate = trailList.Where(t => t.ElevationProfile_Intermediate_ImageID != null).Select(t => t.ElevationProfile_Intermediate_ImageID.Value).ToList();
            var advanced = trailList.Where(t => t.ElevationProfile_Advanced_ImageID != null).Select(t => t.ElevationProfile_Advanced_ImageID.Value).ToList();
            var prof = trailList.Where(t => t.ElevationProfile_Proficiency_ImageID != null).Select(t => t.ElevationProfile_Proficiency_ImageID.Value).ToList();

            var imagesIDList = imagesIDListFullPreview
                .Concat(imagesIDListSmallPreview)
                .Concat(imagesIDListMediumPreview)
                .Concat(elevationProfileIDList)
                .Concat(basic)
                .Concat(intermediate)
                .Concat(advanced)
                .Concat(prof)
                .Distinct().ToList();

            using (var operation = this.DependencyTracker(nameof(this.GetRelatedPreviewImages)))
            {
                var imagesList = this.Blobs.Where(i => imagesIDList.Contains(i.ID)).ToArray();
                return imagesList;
            }
        }
    }
}
