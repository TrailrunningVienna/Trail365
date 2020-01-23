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
        public static StoryDto CreateExcerptandTextStoryWithoutTitleAndImages()
        {
            StoryDto dto = new StoryDto
            {
                ID = new Guid("E081FAC8-3F23-40C3-A8B3-B4B2196B4787"),
                Name = "Name: Kopfhörer am Trail",
                ListAccess = AccessLevel.Public,
            };
            dto.AppendExcerpt("Excerpt: Über Sinn und Unsinn mancher Ausrüstungsgegenstände am Trail");
            dto.AppendText("Text: bla bla bla bla in Line 1" + Environment.NewLine + "blob blob blob in line 2");
            return dto;
        }

        public static StoryDto CreatePicturestory()
        {
            StoryDto dto = new StoryDto
            {
                ID = new Guid("F7E95A0B-D5F9-4B68-8F2F-79B2A7838EC4"),
                Name = "Name: Fotostory",
                ListAccess = AccessLevel.Public,
            };

            dto.AppendExcerpt("Excerpt:Nur Name und Excerpt, kein Text aber viele Bilder");
            dto.AppendImage(ImageDtoProvider.CreateTGHoch().AssignNewID());
            dto.AppendImage(ImageDtoProvider.CreateTGQuer1().AssignNewID());
            dto.AppendImage(ImageDtoProvider.CreateTGQuer2().AssignNewID());
            dto.AppendImage(ImageDtoProvider.CreateKahlenberg().AssignNewID());
            dto.AppendImage(ImageDtoProvider.CreateLindkogel().AssignNewID());
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
                CreateStoryToterGrundWithAllBlockTypesAnd3Pictures()
                , CreateExcerptandTextStoryWithoutTitleAndImages()
                , CreatePicturestory()
                );
        }

        public StoryDto[] All { get; private set; }
    }
}
