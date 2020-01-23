using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trail365.Entities;
using Trail365.Seeds;
using Trail365.UnitTests.TestContext;
using Trail365.ViewModels;
using Xunit;

namespace Trail365.UnitTests.Controllers
{
    [Trait("Category", "BuildVerification")]
    public class StoryUploadControllerTest
    {
        [Fact]
        public void ShouldCreateController()
        {
            using (var host = TestHostBuilder.DefaultForBackend().WithTrailContext().WithIdentityContext().Build())
            {
                using (var hc = host.CreateStoryUploadController())
                {
                    Assert.NotNull(hc);
                    Assert.NotNull(hc.Url);
                }
            }
        }


        [Fact]
        public void ShouldSaveMultiBlockUpload()
        {
            using (var host = TestHostBuilder.DefaultForBackend().WithTrailContext().WithIdentityContext().Build())
            {
                using (var hc = host.CreateStoryUploadController())
                {
                    var model = hc.MultiBlockUpload().ToModel<StoryCreationViewModel>();
                    StatusCodeResult r = (StatusCodeResult)hc.MultiBlockUpload(model);
                    Assert.Equal(hc.NotFound().StatusCode, r.StatusCode);
                    Story story = new Story
                    {
                        ID = model.ID,
                        Name = "xyz",
                        ListAccess = AccessLevel.Public
                    };

                    host.TrailContext.Stories.Add(story);
                    host.TrailContext.SaveChanges();
                    hc.MultiBlockUpload(model);
                }
            }
        }

        [Fact]
        public void ShouldHandleUploadController()
        {
            using (var host = TestHostBuilder.DefaultForBackend().WithTrailContext().WithIdentityContext().UseFileSystemStorage(@"/blob").Build())
            {
                using (Stream s1 = File.OpenRead(Images.KahlenbergAsPng), s2 = File.OpenRead(Images.LindkogelAsJpg), s3 = File.OpenRead(Images.IATF2020AsJpg))
                {
                    using (var hc = host.CreateStoryUploadController())
                    {
                        Guid storyGuid = Guid.NewGuid();
                        var fileCollection = new FormFileCollection
                        {
                            new FormFile(s1, 0, s1.Length, "kahlenberg", "Kahlenberg.PNG"),
                            new FormFile(s2, 0, s2.Length, "LK", "Lindkogel.jpg"),
                            new FormFile(s3, 0, s3.Length, "iatf", "IATF2020.png")
                        };

                        List<StoryBlockFileViewModel> existing = new List<StoryBlockFileViewModel>();
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(existing);
                        Dictionary<string, Microsoft.Extensions.Primitives.StringValues> dict = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
                        {
                            { "existingFiles", new Microsoft.Extensions.Primitives.StringValues(json) }
                        };

                        IFormCollection formCollection = new FormCollection(dict, fileCollection);
                        var result = hc.UploadBlocks(formCollection, storyGuid).ToModel<StoryCreationViewModel>();
                        Assert.Equal(result.ID, storyGuid);
                        Assert.Equal(fileCollection.Count, result.FileInfos.Count);
                        var blockToDelete = result.FileInfos.Last();
                        var result2 = hc.RemoveBlock(blockToDelete.ID, storyGuid).ToModel<StoryCreationViewModel>();
                        Assert.Equal(result2.FileInfos.Count + 1, result.FileInfos.Count);
                    }
                }
            }
        }
    }
}
