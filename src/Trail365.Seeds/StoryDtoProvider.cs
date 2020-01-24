using System;
using Trail365.DTOs;
using Trail365.Entities;

namespace Trail365.Seeds
{
    public class StoryDtoProvider
    {
        public static StoryDtoProvider CreateFromStoryDtos(params StoryDto[] stories)
        {
            if (stories == null) throw new ArgumentNullException(nameof(stories));
            var p = new StoryDtoProvider
            {
                All = stories
            };

            return p;
        }

        /// <summary>
        /// No Images, only Excerpt-Block and Text-Block
        /// </summary>
        /// <returns></returns>
        public static StoryDto CreateExcerptandTextStoryWithoutTitleAndImages(Guid? id=null)
        {
            StoryDto dto = new StoryDto
            {
                ID = new Guid("E081FAC8-3F23-40C3-A8B3-B4B2196B4787"),
                Name = "Name: Kopfhörer am Trail",
                ListAccess = AccessLevel.Public,
            };
            if (id.HasValue)
            {
                dto.ID = id.Value;
            }
            dto.AppendExcerpt("Excerpt: Über Sinn und Unsinn mancher Ausrüstungsgegenstände am [Trail](https:\\www.google.com)");
            dto.AppendText("# Text: bla bla bla bla in Line 1" + Environment.NewLine + "blob blob blob in line 2");
            return dto;
        }

        public static StoryDto CreateUniquePicturestory()
        {
            StoryDto dto = new StoryDto
            {
                ID = Guid.NewGuid(),
                Name = $"Seeded Story {System.DateTime.UtcNow.ToString()}",
                ListAccess = AccessLevel.Public,
            };
            dto.AppendTitle("This should be the Title (without image)");
            dto.AppendExcerpt("This should be the excerpt");
            dto.AppendText($"This is flowing text with {Environment.NewLine}some word wrap and multiline behavior.");
            var image1 = dto.AppendImage(ImageDtoProvider.CreateTGHoch().AssignNewID());
            image1.RawContent = "image1: this should be the image caption (bottom text)";

            var image2 = dto.AppendImage(ImageDtoProvider.CreateTGQuer1().AssignNewID());
            image2.RawContent = "image2_caption";

            var image3 = dto.AppendImage(ImageDtoProvider.CreateTGQuer2().AssignNewID());
            image3.RawContent = "image3_caption";

            return dto;
        }

        public static StoryDto CreateStoryToterGrundWithAllBlockTypesAnd3Pictures()
        {
            StoryDto dto = new StoryDto
            {
                ID = new Guid("5D020827-EA45-4102-9594-3B02230F5916"),
                Name = "Rote Wälder am Toten Grund 01",
                ListAccess = AccessLevel.Public,
            };

            dto.AppendTitle("Title: Geniesser Trail am Toten Grund");
            dto.AppendExcerpt("Excerpt with Link: https://www.wien.gv.at/umwelt/gewaesser/donauinsel/oekologie/nischen.html#accessibletabscontent1-0");
            dto.AppendText("Natur pur in Line 1!" + Environment.NewLine + "Aulandschaft in Line2");
            dto.AppendImage(ImageDtoProvider.CreateTGHoch());
            dto.AppendImage(ImageDtoProvider.CreateTGQuer1());
            dto.AppendImage(ImageDtoProvider.CreateTGQuer2());
            return dto;
        }

        /// <summary>
        /// we need a few realistic looking stories for style and layout refinements
        /// </summary>
        /// <returns></returns>
        public static StoryDtoProvider RealisticStories()
        {
            return CreateFromStoryDtos(
                  CreateStoryToterGrundWithAllBlockTypesAnd3Pictures(),
                  CreateExcerptandTextStoryWithoutTitleAndImages()
                  );
        }

        public static StoryDtoProvider UniqueStories()
        {
            return CreateFromStoryDtos(
                  CreateUniquePicturestory(),
                  CreateExcerptandTextStoryWithoutTitleAndImages(Guid.NewGuid())
                );
        }
        public StoryDto[] All { get; private set; }
    }
}
