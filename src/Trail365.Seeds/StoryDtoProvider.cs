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



        public static StoryDto CreateUnique16Picturestory()
        {
            StoryDto dto = new StoryDto
            {
                ID = Guid.NewGuid(),
                Name = $"Seeded Story with 16 Pictures {System.DateTime.UtcNow.ToString()}",
                ListAccess = AccessLevel.Public,
            };

            dto.AppendExcerpt("This should be the excerpt");
            var r = new Random();
            for (int i = 0; i < 16; i++)
            {
                BlobDto sampleImage;
                var rnd = r.Next(0, 3);
                switch (rnd)
                {
                    case 0: sampleImage = ImageDtoProvider.CreateTGHoch().AssignNewID();
                        break;
                    case 1:
                        sampleImage = ImageDtoProvider.CreateKahlenberg().AssignNewID();
                        break;
                    case 2:
                        sampleImage = ImageDtoProvider.CreateLindkogel().AssignNewID();
                        break;
                    case 3:
                        sampleImage = ImageDtoProvider.CreateIATF2020().AssignNewID();
                        break;
                    default:
                        throw new InvalidOperationException("oops");
                }
                var image = dto.AppendImage(sampleImage);
                image.RawContent = $"image ({i}): this should be the image caption (bottom text)";
            }
            dto.CoverImageID = dto.StoryBlocks[0].Image.ID;
            return dto;
        }


        public static StoryDto CreateUniquePicturestory()
        {
            StoryDto dto = new StoryDto
            {
                ID = Guid.NewGuid(),
                Name = $"Seeded Story Title {System.DateTime.UtcNow.ToString()}",
                ListAccess = AccessLevel.Public,
            };
            dto.AppendExcerpt("This should be the excerpt");
            dto.AppendText($"This is flowing text with {Environment.NewLine}some word wrap and multiline behavior.");
            var image1 = dto.AppendImage(ImageDtoProvider.CreateTGHoch().AssignNewID());
            image1.RawContent = "image1: this should be the image caption (bottom text)";

            var image2 = dto.AppendImage(ImageDtoProvider.CreateTGQuer1().AssignNewID());
            image2.RawContent = "image2_caption";

            var image3 = dto.AppendImage(ImageDtoProvider.CreateTGQuer2().AssignNewID());
            image3.RawContent = "image3_caption";
            dto.CoverImageID = image3.Image.ID;
            return dto;
        }

        public static StoryDto CreateStoryToterGrundWithAllBlockTypesAnd3Pictures()
        {
            StoryDto dto = new StoryDto
            {
                ID = new Guid("5D020827-EA45-4102-9594-3B02230F5916"),
                Name = "Title Rote Wälder am Toten Grund 01",
                ListAccess = AccessLevel.Public,
            };
            dto.AppendExcerpt("Excerpt with Link: https://www.wien.gv.at/umwelt/gewaesser/donauinsel/oekologie/nischen.html#accessibletabscontent1-0");
            dto.AppendText("Natur pur in Line 1!" + Environment.NewLine + "Aulandschaft in Line2");
            dto.AppendImage(ImageDtoProvider.CreateTGHoch());
            dto.AppendImage(ImageDtoProvider.CreateTGQuer1());
            dto.AppendImage(ImageDtoProvider.CreateTGQuer2());
            dto.CoverImageID = dto.StoryBlocks[2].Image.ID;
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
                  CreateExcerptandTextStoryWithoutTitleAndImages(Guid.NewGuid()),
                  CreateUnique16Picturestory()
                );
        }
        public StoryDto[] All { get; private set; }
    }
}
