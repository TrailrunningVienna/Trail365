using System;
using Trail365.DTOs;
using Trail365.Seeds;
using Trail365.Services;
using Trail365.UnitTests.Utils;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class BlobServiceExtensionTests
    {
        [Fact]
        public void Should_CreateStoryFromStoryDto()
        {
            using (var host = TestHostBuilder.Empty().Build())
            {
                var sample = StoryDtoProvider.CreateStoryToterGrundWithAllBlockTypesAnd3Pictures();
                var result = host.BlobService.CreateStoryFromStoryDto(sample, HelperExtensions.EmptyUrlHelper);
                Assert.Equal(sample.StoryBlocks.Count, result.Item1.StoryBlocks.Count);
                Assert.Equal(sample.Name, result.Item1.Name);
                Assert.Equal(sample.Name, result.Item2.Name);
            }
        }

        [Fact]
        public void Should_CreateImageFromImageDto()
        {
            using (var host = TestHostBuilder.Empty().Build())
            {
                var sample = ImageDtoProvider.CreateTGHoch();
                var result = host.BlobService.CreateBlobFromBlobDto(sample, HelperExtensions.EmptyUrlHelper);
                Assert.NotEmpty(result.Item2.Url);
                Assert.Equal(sample.SubFolder, result.Item2.SubFolder);
                Assert.Equal(sample.SubFolder, result.Item1.FolderName);
                Assert.True(result.Item2.ImageWidth.HasValue);
                Assert.True(result.Item2.ImageHeight.HasValue);
            }
        }

        [Fact]
        public void CreateBlobFromBlobDto_ArgumentCheck_1()
        {
            var blobService = BlobServiceFactory.CreateLocalInstance();
            BlobDto dto = new BlobDto();
            Assert.Throws<InvalidOperationException>(() => blobService.CreateBlobFromBlobDto(dto, HelperExtensions.EmptyUrlHelper));

            dto.OriginalFileName = "test.xyz";
            Assert.Throws<InvalidOperationException>(() => blobService.CreateBlobFromBlobDto(dto, HelperExtensions.EmptyUrlHelper));

            dto.Data = new byte[] { 0, 1, 2, 3, 4 };

            var result = blobService.CreateBlobFromBlobDto(dto, HelperExtensions.EmptyUrlHelper);
            Assert.NotNull(result.Item1);
            Assert.NotNull(result.Item2);

            Assert.False(string.IsNullOrEmpty(result.Item1.ContentHash));
            Assert.Equal(dto.Data.Length, result.Item1.StorageSize.Value);

            Assert.Equal(result.Item1.ID, result.Item2.ID);
            Assert.Equal(dto.ID, result.Item1.ID);
            Assert.Equal(dto.OriginalFileName, result.Item1.OriginalFileName);
            Assert.Equal(dto.OriginalFileName, result.Item2.OriginalFileName);
        }

        [Fact]
        public void CreateBlobFromBlobDto_ArgumentCheck_2()
        {
            var blobService = BlobServiceFactory.CreateLocalInstance();
            BlobDto dto = new BlobDto
            {
                OriginalFileName = null,
                SubFolder = "jpg",
                Data = System.IO.File.ReadAllBytes(Images.IATF2020AsJpg)
            };

            //we need a valid picture because ImageSize calculation is triggered for file extension!
            var result = blobService.CreateBlobFromBlobDto(dto, HelperExtensions.EmptyUrlHelper);
            Assert.NotNull(result);

            Assert.False(string.IsNullOrEmpty(result.Item1.ContentHash));
            Assert.Equal(dto.Data.Length, result.Item1.StorageSize.Value);

            Assert.Equal(SupportedMimeType.ImageJpg, result.Item1.MimeType);
            Assert.Equal(SupportedMimeType.ImageJpg, result.Item2.MimeType);
            Assert.Equal(result.Item1.ImageHeight, result.Item2.ImageHeight);
            Assert.Equal(result.Item1.ImageWidth, result.Item2.ImageWidth);
            Assert.True(result.Item1.ImageHeight.HasValue);
            Assert.True(result.Item1.ImageWidth.HasValue);
        }

        [Fact]
        public void CreateBlobFromBlobDto_ArgumentCheck_3()
        {
            var blobService = BlobServiceFactory.CreateLocalInstance();
            BlobDto dto = new BlobDto { OriginalFileName = null, SubFolder = "ukn", Data = new byte[] { 0, 1, 2, 3, 4 } };

            var result = blobService.CreateBlobFromBlobDto(dto, HelperExtensions.EmptyUrlHelper);
            Assert.NotNull(result);

            Assert.False(string.IsNullOrEmpty(result.Item1.ContentHash));
            Assert.Equal(dto.Data.Length, result.Item1.StorageSize.Value);

            Assert.Equal(SupportedMimeType.Application, result.Item1.MimeType);
            Assert.Equal(SupportedMimeType.Application, result.Item2.MimeType);
            Assert.Equal(result.Item1.ImageHeight, result.Item2.ImageHeight);
            Assert.Equal(result.Item1.ImageWidth, result.Item2.ImageWidth);
            Assert.False(result.Item1.ImageHeight.HasValue);
            Assert.False(result.Item1.ImageWidth.HasValue);
        }

        [Fact]
        public void CreateBlobFromBlobDto_ArgumentCheck_4()
        {
            var blobService = BlobServiceFactory.CreateLocalInstance();
            BlobDto dto = new BlobDto
            {
                OriginalFileName = "test.ggg",
                SubFolder = null,
                Data = new byte[] { 0, 1, 2, 3, 4 }
            };

            var result = blobService.CreateBlobFromBlobDto(dto, HelperExtensions.EmptyUrlHelper);
            Assert.NotNull(result);

            Assert.False(string.IsNullOrEmpty(result.Item1.ContentHash));
            Assert.Equal(dto.Data.Length, result.Item1.StorageSize.Value);
            Assert.Equal(result.Item1.FolderName, result.Item2.SubFolder);
            Assert.Equal("ggg", result.Item1.FolderName);
            Assert.Equal(SupportedMimeType.Application, result.Item1.MimeType);
            Assert.Equal(SupportedMimeType.Application, result.Item2.MimeType);
            Assert.Equal(result.Item1.ImageHeight, result.Item2.ImageHeight);
            Assert.Equal(result.Item1.ImageWidth, result.Item2.ImageWidth);
            Assert.False(result.Item1.ImageHeight.HasValue);
            Assert.False(result.Item1.ImageWidth.HasValue);
        }

        [Fact]
        public void CreateBlobFromBlobDto_ArgumentCheck_5()
        {
            var blobService = BlobServiceFactory.CreateLocalInstance();
            BlobDto dto = new BlobDto
            {
                OriginalFileName = "test.ggg",
                SubFolder = "abcde",
                Data = new byte[] { 0, 1, 2, 3, 4 }
            };

            var result = blobService.CreateBlobFromBlobDto(dto, HelperExtensions.EmptyUrlHelper);
            Assert.NotNull(result);

            Assert.False(string.IsNullOrEmpty(result.Item1.ContentHash));
            Assert.Equal(dto.Data.Length, result.Item1.StorageSize.Value);
            Assert.Equal(result.Item1.FolderName, result.Item2.SubFolder);
            Assert.Equal(dto.SubFolder, result.Item1.FolderName);
            Assert.Equal(SupportedMimeType.Application, result.Item1.MimeType);
            Assert.Equal(SupportedMimeType.Application, result.Item2.MimeType);
            Assert.Equal(result.Item1.ImageHeight, result.Item2.ImageHeight);
            Assert.Equal(result.Item1.ImageWidth, result.Item2.ImageWidth);
            Assert.False(result.Item1.ImageHeight.HasValue);
            Assert.False(result.Item1.ImageWidth.HasValue);
        }

        [Theory]
        [InlineData(@"a\b")]
        [InlineData(@"a/b")]
        [InlineData(@"a.b")]
        [InlineData(@"a b")]
        public void CreateBlobFromBlobDto_InvalidFolderNames(string folderName)
        {
            var blobService = BlobServiceFactory.CreateLocalInstance();
            BlobDto dto = new BlobDto
            {
                OriginalFileName = "test.ggg",
                SubFolder = folderName,
                Data = new byte[] { 0, 1, 2, 3, 4 }
            };
            Assert.Throws<InvalidOperationException>(() => blobService.CreateBlobFromBlobDto(dto, HelperExtensions.EmptyUrlHelper));
        }

        [Theory]
        [InlineData(@"a\b")]
        [InlineData(@"a/b")]
        [InlineData(@"a b")]
        public void CreateBlobFromBlobDto_InvalidExtensionNames(string ext)
        {
            var blobService = BlobServiceFactory.CreateLocalInstance();
            BlobDto dto = new BlobDto
            {
                OriginalFileName = "test." + ext,
                SubFolder = null,
                Data = new byte[] { 0, 1, 2, 3, 4 }
            };
            Assert.Throws<InvalidOperationException>(() => blobService.CreateBlobFromBlobDto(dto, HelperExtensions.EmptyUrlHelper));
        }

        [Theory]
        //[InlineData(@"a\b")]
        //[InlineData(@"a/b")]
        [InlineData(@"a b")]
        public void CreateBlobFromBlobDto_InvalidExtensionNames_2(string ext)
        {
            var blobService = BlobServiceFactory.CreateLocalInstance();
            BlobDto dto = new BlobDto
            {
                OriginalFileName = "test." + ext,
                SubFolder = "valid",
                Data = new byte[] { 0, 1, 2, 3, 4 }
            };
            Assert.Throws<InvalidOperationException>(() => blobService.CreateBlobFromBlobDto(dto, HelperExtensions.EmptyUrlHelper));
        }
    }
}
