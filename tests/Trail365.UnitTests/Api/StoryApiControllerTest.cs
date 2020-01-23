using System.Collections.Generic;
using System.Linq;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Seeds;
using Trail365.UnitTests.TestContext;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Api
{
    [Trait("Category", "BuildVerification")]
    public class StoryApiControllerTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public StoryApiControllerTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        public static IEnumerable<object[]> GetSeeds()
        {
            List<object[]> seeds = new List<object[]>
            {
                new StoryDto[][] { StoryDtoProvider.RealisticStories().All}
            };
            return seeds;
        }

        [Theory]
        [MemberData(nameof(GetSeeds))]
        public void ShouldCreateListAndDeleteStories(StoryDto[] testData)
        {
            using (var host = TestHostBuilder.Empty().UseTestOutputHelper(this.OutputHelper).UseFileSystemStorage().UseStaticAuthenticationAsAdmin().WithTrailContext().Build())
            {
                var hc = host.CreateStoryApiController();
                foreach (var dto in testData)
                {
                    var result = hc.PostStory(dto);
                    Assert.NotNull(result);
                }
                Assert.Equal(testData.Length, host.TrailContext.Stories.ToList().Count);
                var results = hc.GetStories().Value.ToArray();
                foreach (var evDto in results)
                {
                    hc.DeleteStory(evDto.ID);
                }
                Assert.Empty(host.TrailContext.Stories.ToList());
            }
        }

        [Fact]
        public void ShouldCreateStoryAndComponents()
        {
            var sampleDto = StoryDtoProvider.CreateStoryToterGrundWithAllBlockTypesAnd3Pictures();
            using (var host = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).UseFileSystemStorage().UseStaticAuthenticationAsAdmin().WithTrailContext().Build())
            {
                var hc = host.CreateStoryApiController();
                hc.PostStory(sampleDto);
                Assert.Equal(sampleDto.StoryBlocks.Count, host.TrailContext.StoryBlocks.ToList().Count);
                Assert.Equal(sampleDto.StoryBlocks.Where(bl => bl.Image != null).Count(), host.TrailContext.Blobs.ToList().Count);
                Assert.Single(host.TrailContext.Stories.ToList());
                var story = host.TrailContext.Stories.Single();
                Assert.Equal(StoryStatus.Draft, story.Status);
                var results = hc.GetStories().Value.ToArray();
            }
        }
    }
}
