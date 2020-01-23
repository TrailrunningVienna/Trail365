using System;
using Trail365.Entities;

namespace Trail365.DTOs
{
    public static class DTOExtension
    {
        /// <summary>
        /// properties Data and imageType are impacted, NO Other!
        /// </summary>
        /// <param name="image"></param>
        /// <param name="downloadService"></param>
        /// <param name="url"></param>

        public static StoryDto AppendExcerpt(this StoryDto story, string excerpt)
        {
            if (story == null) throw new ArgumentNullException(nameof(story));
            if (string.IsNullOrEmpty(excerpt)) throw new ArgumentNullException(nameof(excerpt));
            story.StoryBlocks.Add(new StoryBlockDto
            {
                BlockType = StoryBlockType.Excerpt,
                RawContent = excerpt
            });
            return story;
        }

        public static StoryDto AppendText(this StoryDto story, string text)
        {
            if (story == null) throw new ArgumentNullException(nameof(story));
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));
            story.StoryBlocks.Add(new StoryBlockDto
            {
                BlockType = StoryBlockType.Text,
                RawContent = text
            });
            return story;
        }

        public static StoryDto AppendImage(this StoryDto story, BlobDto image)
        {
            if (story == null) throw new ArgumentNullException(nameof(story));
            if (image == null) throw new ArgumentNullException(nameof(image));

            story.StoryBlocks.Add(new StoryBlockDto
            {
                BlockType = StoryBlockType.Image,
                Image = image
            });

            return story;
        }

        public static StoryDto AppendTitle(this StoryDto story, string title)
        {
            if (story == null) throw new ArgumentNullException(nameof(story));
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));
            story.StoryBlocks.Add(new StoryBlockDto
            {
                BlockType = StoryBlockType.Title,
                RawContent = title
            });
            return story;
        }

        public static BlobDto AssignNewID(this BlobDto imageDto)
        {
            imageDto.ID = Guid.NewGuid();
            return imageDto;
        }

        /// <summary>
        /// Image property is 100% IGNORED!
        /// </summary>
        /// <param name="blockDto"></param>
        /// <param name="owningStory"></param>
        /// <returns></returns>
        public static StoryBlock ToStoryBlockWithoutImage(this StoryBlockDto blockDto, Story owningStory)
        {
            if (owningStory == null) throw new ArgumentNullException(nameof(owningStory));
            if (blockDto == null) throw new ArgumentNullException(nameof(blockDto));

            StoryBlock sp = new StoryBlock
            {
                ID = blockDto.ID,
                Story = owningStory,
                StoryID = owningStory.ID,
                BlockType = blockDto.BlockType,
                RawContent = blockDto.RawContent
            };

            return sp;
        }

        public static Place ToPlace(this PlaceDto placeDto)
        {
            if (placeDto == null) throw new ArgumentNullException(nameof(placeDto));
            Place p = new Place
            {
                ID = placeDto.ID,
                Name = placeDto.Name,
                City = placeDto.City,
                Country = placeDto.Country,
                CountryTwoLetterISOCode = placeDto.CountryTwoLetterISOCode,
                Zip = placeDto.Zip,
                Longitude = placeDto.Longitude,
                Latitude = placeDto.Latitude
            };
            if (string.IsNullOrEmpty(p.Name))
            {
                p.Name = p.City;
            }
            return p;
        }

        public static Story ToStoryWithoutBlocks(this StoryDto storyDto)
        {
            if (storyDto == null) throw new ArgumentNullException(nameof(storyDto));
            Story story = new Story
            {
                ID = storyDto.ID,
                Name = storyDto.Name,
                ListAccess = storyDto.ListAccess,
            };
            return story;
        }

        public static StoryDto ToStoryDto(this Story model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            return new StoryDto()
            {
                ID = model.ID,
                Name = model.Name,
                ListAccess = model.ListAccess,
            };
        }

        public static TrailDto ToTrailDto(this Trail model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            return new TrailDto()
            {
                ID = model.ID,
                Name = model.Name,
                Description = model.Description,
                InternalDescription = model.InternalDescription,
                ListAccess = model.ListAccess,
                GpxDownloadAccess = model.GpxDownloadAccess,
                Excerpt = model.Excerpt,
                DistanceMeters = model.DistanceMeters,
                AscentMeters = model.AscentMeters,
                DescentMeters = model.DescentMeters,
                MinimumAltitude = model.MinimumAltitude,
                MaximumAltitude = model.MaximumAltitude
            };
        }

        public static Event ToEvent(this EventDto eventDto)
        {
            if (eventDto == null) throw new ArgumentNullException(nameof(eventDto));
            Event e = new Event
            {
                ID = eventDto.ID,
                OrganizerPermalink = eventDto.OrganizerPermalink,
                Name = eventDto.Name,
                StartTimeUtc = eventDto.StartTimeUtc,
                EndTimeUtc = eventDto.EndTimeUtc,
                ListAccess = eventDto.ListAccess,
                Excerpt = eventDto.Excerpt,
                Description = eventDto.Description,
                ExternalID = eventDto.ExternalID,
                ExternalSource = eventDto.ExternalSource,
                ModifiedUtc = eventDto.ModifiedUtc
            };
            if (eventDto.CreatedUtc.HasValue)
            {
                e.CreatedUtc = eventDto.CreatedUtc.Value;
            }
            return e;
        }

        public static PlaceDto ToPlaceDto(this Place entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            return new PlaceDto
            {
                ID = entity.ID,
                Name = entity.Name,
                City = entity.City,
                Country = entity.Country,
                CountryTwoLetterISOCode = entity.CountryTwoLetterISOCode,
                Zip = entity.Zip,
                Longitude = entity.Longitude,
                Latitude = entity.Latitude,
            };
        }

        public static EventDto ToEventDto(this Event entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            return new EventDto()
            {
                ID = entity.ID,
                Name = entity.Name,
                Description = entity.Description,
                ListAccess = entity.ListAccess,
                Excerpt = entity.Excerpt,
                StartTimeUtc = entity.StartTimeUtc,
                EndTimeUtc = entity.EndTimeUtc,
                OwnerID = entity.OwnerID,
                OrganizerPermalink = entity.OrganizerPermalink,
                ExternalID = entity.ExternalID,
                ExternalSource = entity.ExternalSource,
                CreatedUtc = entity.CreatedUtc,
                ModifiedUtc = entity.ModifiedUtc
            };
        }
    }
}
