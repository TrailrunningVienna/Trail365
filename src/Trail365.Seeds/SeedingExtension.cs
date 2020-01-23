using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Trail365.Data;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Services;

namespace Trail365.Seeds
{
    public static class SeedingExtension
    {
        public static void SeedTrails(this TrailContext context, Seeds.TrailDtoProvider trailDtoProvider, BlobService blobService, IUrlHelper helper)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (blobService == null) throw new ArgumentNullException(nameof(context));
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (trailDtoProvider == null) throw new ArgumentNullException(nameof(trailDtoProvider));
            //assume database is empty and migrated (latest version)
            context.SeedTrails(blobService, trailDtoProvider.All, helper);
            context.SaveChanges();
        }

        public static void SeedEvents(this TrailContext context, Seeds.EventDtoProvider eventDtoProvider, BlobService blobService, IUrlHelper helper)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (blobService == null) throw new ArgumentNullException(nameof(context));
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (eventDtoProvider == null) throw new ArgumentNullException(nameof(eventDtoProvider));
            context.AddEvent(blobService, helper, eventDtoProvider.All);
            context.SaveChanges();
        }

        public static void SeedPlaces(this TrailContext context, Seeds.PlaceDtoProvider placeDtoProvider, IUrlHelper helper)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (placeDtoProvider == null) throw new ArgumentNullException(nameof(placeDtoProvider));
            //assume database is empty and migrated (latest version)

            List<Place> batch = new List<Place>();
            foreach (var seed in placeDtoProvider.All)
            {
                var item = seed.ToPlace();
                batch.Add(item);

            }
            string userName = "system";
            var timeStamp = DateTime.UtcNow;
            //set createdUser explicitely because there is no current thread user available
            batch.ForEach(st => st.CreatedByUser = userName);
            batch.ForEach(st => st.CreatedUtc = timeStamp);
            context.Places.AddRange(batch);
            var changes = context.SaveChanges();
        }

        public static void SeedStories(this TrailContext context, Seeds.StoryDtoProvider storyDtoProvider, BlobService blobService, IUrlHelper helper, StoryStatus? status = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (blobService == null) throw new ArgumentNullException(nameof(context));
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (storyDtoProvider == null) throw new ArgumentNullException(nameof(storyDtoProvider));
            //assume database is empty and migrated (latest version)

            List<Story> batch = new List<Story>();
            foreach (var seed in storyDtoProvider.All)
            {
                var result = blobService.CreateStoryFromStoryDto(seed, helper);
                if (status.HasValue)
                {
                    result.Item1.Status = status.Value;
                }
                batch.Add(result.Item1);
            }
            //set createdUser explicitely because there is no current thread user available
            string userName = "system";
            batch.ForEach(st => st.CreatedByUser = userName);
            batch.SelectMany(st => st.StoryBlocks).ToList().ForEach(b => b.CreatedByUser = userName);
            batch.SelectMany(st => st.StoryBlocks).Where(sb => sb.Image != null).ToList().ForEach(sb1 => sb1.Image.CreatedByUser = userName);
            context.Stories.AddRange(batch);
            var changes = context.SaveChanges();
        }
    }
}
