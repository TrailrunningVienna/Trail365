using System;
using System.Linq;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Seeds;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Views
{
    [Trait("Category", "BuildVerification")]
    [Trait("Category", "Routing")]
    public class StoryDetailEditorRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public StoryDetailEditorRouteTest(ITestOutputHelper outputHelper)
        {
            this.OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Fact]
        public void ShouldRenderStoryDetailEditor()
        {
            var storyDtoProvider = StoryDtoProvider.CreateFromStoryDtos(StoryDtoProvider.CreateExcerptandTextStoryWithoutTitleAndImages());

            using (var host = TestHostBuilder.DefaultForBackend(OutputHelper).Build())
            {
                foreach (var seed in storyDtoProvider.All)
                {
                    Story story = seed.ToStoryWithoutBlocks();
                    story.StoryBlocks.AddRange(seed.StoryBlocks.Select(sb => sb.ToStoryBlockWithoutImage(story)));
                    host.TrailContext.Stories.Add(story);
                }

                host.TrailContext.SaveChanges();

                foreach (var seed in storyDtoProvider.All)
                {
                    var singleBlock = seed.StoryBlocks.First();
                    var result = host.GetFromServer($"StoryDetailEditor/Edit/{singleBlock.ID.ToString()}");
                    result.EnsureSuccessStatusCode();
                }
            }
        }
    }
}
