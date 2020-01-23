using System.Linq;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Seeds;
using Trail365.UnitTests.TestContext;
using Trail365.ViewModels;
using Xunit;

namespace Trail365.UnitTests.Controllers
{
    [Trait("Category", "BuildVerification")]
    public class HomeControllerStoryTest
    {
        [Fact]
        public void ShouldReturnSingleStory()
        {
            using (var host = TestHostBuilder.DefaultForBackend().Build())
            {
                var seed = StoryDtoProvider.CreateStoryToterGrundWithAllBlockTypesAnd3Pictures();
                var apiController = host.CreateStoryApiController();
                var result = apiController.PostStory(seed);

                Assert.Equal(seed.StoryBlocks.Where(bl => bl.Image != null).Count(), host.TrailContext.Blobs.Count());
                Assert.Equal(seed.StoryBlocks.Count, host.TrailContext.StoryBlocks.Count());

                StoryQueryFilter qf = new StoryQueryFilter(EntityExtension.AllAccessLevels, false)
                {
                    IncludeBlocks = true
                };

                var story = host.TrailContext.GetStoriesByFilter(qf).FirstOrDefault();

                Assert.Equal(seed.StoryBlocks.Count, story.StoryBlocks.Count);
                Assert.Equal(seed.StoryBlocks.Count, story.StoryBlocks.Count);
                var controller = host.CreateStoryController();
                var viewModel = controller.Index(new StoryRequestViewModel { ID = seed.ID }).ToModel<StoryViewModel>();
            }
        }
    }
}
