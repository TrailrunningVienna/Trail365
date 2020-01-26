using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trail365.Data;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Graphics;
using Trail365.Internal;
using Trail365.ViewModels;

namespace Trail365.Services
{
    public static class BlobServiceExtension
    {
        public static StoryBlockType GetProposedStoryBlockType(IFormFile file, out string extension)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            extension = Path.GetExtension(file.FileName).ToLowerInvariant().Trim('.');

            if (string.IsNullOrEmpty(extension))
            {
                throw new InvalidOperationException($"File extension could not be retrieved ({file.FileName})");
            }

            var mimeType = SupportedMimeType.CalculateMimeTypeFromFileName(file.FileName);
            var result = StoryBlockType.Text;
            if (SupportedMimeType.IsTypeWithImageSize(mimeType))
            {
                result = StoryBlockType.Image;
            }
            return result;
        }

        public static StoryBlock CreateStoryBlockFromFile(this BlobService blobService, IFormFile file, Guid storyID, StoryBlockType blockType, string extension, IUrlHelper helper)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException(nameof(extension));

            StoryBlock result = new StoryBlock
            {
                StoryID = storyID,
                BlockType = blockType,
            };

            if (blockType == StoryBlockType.Image)
            {
                using (var stream = file.OpenReadStream())
                {
                    result.Image = blobService.CreateBlobFromStream(stream, file.FileName, extension, helper);
                }
                result.ImageID = result.Image.ID;
            }
            else
            {
                using (var stream = file.OpenReadStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        result.RawContent = reader.ReadToEnd();
                    }
                }
            }
            return result;
        }

        public static StoryBlockFileViewModel[] AppendBlocks(this BlobService blobService, TrailContext context, Story story, IFormFile[] files, IUrlHelper helper)
        {
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (files == null) throw new ArgumentNullException(nameof(files));
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));
            if (story == null) throw new ArgumentNullException(nameof(story));

            List<StoryBlockFileViewModel> results = new List<StoryBlockFileViewModel>();

            int sortValue = 0;

            if (story.StoryBlocks.Count > 0)
            {
                sortValue = story.StoryBlocks.Select(sb => sb.SortOrder).Max();
            }

            foreach (var file in files)
            {
                var blockType = BlobServiceExtension.GetProposedStoryBlockType(file, out string ext);
                var block = blobService.CreateStoryBlockFromFile(file, story.ID, blockType, ext, helper);

                sortValue += 1;

                block.SortOrder = sortValue;
                context.StoryBlocks.Add(block);
                story.StoryBlocks.Add(block);

                if (block.Image != null)
                {
                    context.Blobs.Add(block.Image);
                }
                Guard.Assert(file.FileName == block.Image.OriginalFileName);
                var vm = new StoryBlockFileViewModel
                {
                    ID = block.ID,
                    FileName = file.FileName,
                    AbsoluteUrl = block.Image?.Url,
                    Name = file.Name,
                    Extension = ext,
                };
                results.Add(vm);
            }
            return results.ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="blobService"></param>
        /// <param name="storyDto"></param>
        /// <param name="helper"></param>
        /// <returns>Item2 is NOT the inputDTO, but a fresh created DTO from resulting (nonDTO) Story</returns>
        public static Tuple<Story, StoryDto> CreateStoryFromStoryDto(this BlobService blobService, StoryDto storyDto, IUrlHelper helper)
        {
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (storyDto == null) throw new ArgumentNullException(nameof(storyDto));
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));

            var resultDto = new StoryDto
            {
                ID = storyDto.ID,
                Name = storyDto.Name,
                ListAccess = storyDto.ListAccess,
            };

            Story resultStory = storyDto.ToStoryWithoutBlocksAndImages();


            var withOrdering = storyDto.StoryBlocks.Where(sb => sb.SortOrder != 0).ToArray();

            if (withOrdering.Length == 0)
            {
                //use the ordering coming from dtoModel.
                int ordering = 0;
                storyDto.StoryBlocks.ForEach(sb =>
               {
                   sb.SortOrder = ordering;
                   ordering += 1;
               });
            }
            foreach (var block in storyDto.StoryBlocks)
            {
                StoryBlock sp = block.ToStoryBlockWithoutImage(resultStory);

                StoryBlockDto resultBlockDto = new StoryBlockDto { ID = block.ID };

                if (block.Image != null)
                {
                    var result = blobService.CreateBlobFromBlobDto(block.Image, helper);
                    resultBlockDto.Image = result.Item2;
                    sp.Image = result.Item1;
                    sp.ImageID = result.Item1.ID;
                }
                resultStory.StoryBlocks.Add(sp);
            }

            if (storyDto.CoverImageID.HasValue && (storyDto.CoverImageID.Value != Guid.Empty))
            {
                var exists = resultStory.StoryBlocks.Where(sb => sb.ImageID == storyDto.CoverImageID).Any();
                if (!exists)
                {
                    throw new InvalidOperationException("CoverImage does not exists");
                }
                resultStory.CoverImageID = storyDto.CoverImageID.Value;
            }


            return new Tuple<Story, StoryDto>(resultStory, resultDto);
        }

        public static BlobMapping UploadStream(this BlobService blobService, Stream streamData, Guid blobGUID, string folderName, string fileExtension, IUrlHelper helper)
        {
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));
            if (streamData == null) throw new ArgumentNullException(nameof(streamData));
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            Guard.Assert(streamData.CanSeek);
            Guard.Assert(streamData.CanRead);
            Guard.Assert(streamData.Position == 0);
            Guard.Assert(streamData.CanRead);
            Guard.Assert(streamData.Position == 0);
            Guard.Assert(streamData.CanSeek);

            Guard.Assert(folderName == Utils.GetValidFolderName(folderName));

            var result = new BlobMapping
            {
                Url = blobService.Upload(blobGUID, folderName, fileExtension, streamData, streamData.Length, helper),
                StorageSize = Convert.ToInt32(streamData.Length),
                FolderName = folderName,
                MimeType = SupportedMimeType.CalculateMimeTypeFromFileExtension(fileExtension),
            };
            streamData.Position = 0;
            result.ContentHash = HashUtils.CalculateHash(streamData);
            bool hasImageSize = SupportedMimeType.IsTypeWithImageSize(result.MimeType);
            if (hasImageSize)
            {
                streamData.Position = 0;
                var sz = ImageAnalyzer.GetSize(streamData);
                result.ImageWidth = sz.Width;
                result.ImageHeight = sz.Height;
            }
            Guard.AssertNotNull(result.Url);
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="blobService"></param>
        /// <param name="source"></param>
        /// <param name="originalFileNameOrDefault"></param>
        /// <param name="extension"></param>
        /// <param name="helper"></param>
        /// <returns></returns>
        private static Blob CreateBlobFromStream(this BlobService blobService, Stream source, string originalFileNameOrDefault, string extension, IUrlHelper helper)
        {
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));

            if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException(nameof(extension));

            string folderName = Utils.GetValidFolderName(extension);

            var image = new Blob();

            if (source.Position != 0)
            {
                throw new InvalidOperationException("Unexpected STream Position");
            }

            if (!source.CanSeek)
            {
                throw new InvalidOperationException($"CanSeek not available for {source.GetType().FullName}");
            }
            var mapping = UploadStream(blobService, source, image.ID, folderName, extension, helper);
            source.Position = 0;
            mapping.ContentHash = HashUtils.CalculateHash(source);
            mapping.ApplyToBlob(image);

            if (!string.IsNullOrEmpty(originalFileNameOrDefault))
            {
                image.OriginalFileName = originalFileNameOrDefault;
            }

            image.CreatedByUser = System.Threading.Thread.CurrentPrincipal?.Identity.Name;
            return image;
        }

        public static Tuple<Blob, BlobDto> CreateBlobFromBlobDto(this BlobService blobService, BlobDto blobDto, IUrlHelper helper)
        {
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (blobDto == null) throw new ArgumentNullException(nameof(blobDto));
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));

            Blob resultingBlob;
            BlobDto dto;

            if ((blobDto.Data == null) && (string.IsNullOrEmpty(blobDto.Url) == false) && (blobDto.ImageWidth.HasValue) && (blobDto.ImageHeight.HasValue))
            {
                throw new NotImplementedException("NI001");
                //resultingBlob = Blob.FromDto(blobDto);
                //resultingBlob.Validate();
            }
            else
            {
                string subFolder = blobDto.SubFolder;
                string extension = subFolder;
                if (string.IsNullOrEmpty(subFolder))
                {
                    extension = Path.GetExtension(blobDto.OriginalFileName);
                    if (string.IsNullOrEmpty(extension))
                    {
                        throw new InvalidOperationException("File extension cannot be calculated from originalFilename 1");
                    }
                    subFolder = Utils.GetValidFolderName(extension);
                }
                else
                {
                    if (!string.IsNullOrEmpty(blobDto.OriginalFileName))
                    {
                        extension = Path.GetExtension(blobDto.OriginalFileName);
                        if (string.IsNullOrEmpty(extension))
                        {
                            throw new InvalidOperationException("File extension cannot be calculated from originalFilename2 ");
                        }
                    }
                    subFolder = Utils.GetValidFolderName(subFolder);
                }
                if (blobDto.Data == null)
                {
                    throw new InvalidOperationException("BlobDto.Data is not set");
                }
                var mapping = UploadBytes(blobService, blobDto.Data, blobDto.ID, subFolder, extension, helper);

                resultingBlob = new Blob
                {
                    ID = blobDto.ID,
                    OriginalFileName = blobDto.OriginalFileName,
                };

                mapping.ApplyToBlob(resultingBlob);
                resultingBlob.Validate();
                resultingBlob.CreatedByUser = System.Threading.Thread.CurrentPrincipal?.Identity.Name;
            }
            Guard.Assert(blobDto.ID == resultingBlob.ID);
            dto = BlobDto.FromBlob(resultingBlob);
            return new Tuple<Blob, BlobDto>(resultingBlob, dto);
        }

        public static void UploadBytesAsGpx(this BlobService blobService, byte[] gpxData, Blob gpxBlob, IUrlHelper helper)
        {
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));
            if (gpxData == null) throw new ArgumentNullException(nameof(gpxData));
            if (gpxBlob == null) throw new ArgumentNullException(nameof(gpxBlob));
            var mapping = UploadBytes(blobService, gpxData, gpxBlob.ID, GpxFileType, helper);
            mapping.ApplyToBlob(gpxBlob);
        }

        /// <summary>
        /// gpx (without dot prefix)
        /// </summary>
        public static readonly string GpxFileType = "gpx";

        public static BlobMapping UploadXml(this BlobService blobService, string xmlData, Guid blobGUID, IUrlHelper helper)
        {
            byte[] gpxData = Encoding.UTF8.GetBytes(xmlData);
            string fileExtension = GpxFileType;
            var result = UploadBytes(blobService, gpxData, blobGUID, fileExtension, helper);
            return result;
        }

        public static BlobMapping UploadBytes(this BlobService blobService, byte[] bufferData, Guid blobGUID, string fileExtension, IUrlHelper helper)
        {
            string folderName = Utils.GetValidFolderName(fileExtension);
            return UploadBytes(blobService, bufferData, blobGUID, folderName, fileExtension, helper);
        }

        private static BlobMapping UploadBytes(this BlobService blobService, byte[] bufferData, Guid blobGUID, string folderName, string fileExtension, IUrlHelper helper)
        {
            if (blobService == null) throw new ArgumentNullException(nameof(blobService));
            if (bufferData == null) throw new ArgumentNullException(nameof(bufferData));
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (string.IsNullOrEmpty(fileExtension)) throw new ArgumentNullException(nameof(fileExtension));
            if (string.IsNullOrEmpty(folderName)) throw new ArgumentNullException(nameof(folderName));
            using (var stream = new MemoryStream(bufferData))
            {
                var result = blobService.UploadStream(stream, blobGUID, folderName, fileExtension, helper);
                return result;
            };
        }
    }
}
