using System;
using Trail365.DTOs;
using Trail365.Entities;
using Trail365.Seeds;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Views
{
    [Trait("Category", "BuildVerification")]
    [Trait("Category", "Routing")]
    public class StoryViewTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public StoryViewTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Fact]
        public void ShouldRenderEmptyStoryIndex()
        {
            using (var server = TestHostBuilder.DefaultForFrontendAsAdmin(this.OutputHelper).Build())
            {
                server.GetFromServer(RouteName.StoryNewsIndex).EnsureSuccessStatusCode();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TextOnlyStory(bool noConsent)
        {
            var seeds = StoryDtoProvider.CreateFromStoryDtos(StoryDtoProvider.CreateExcerptandTextStoryWithoutTitleAndImages());
            this.StoryUnderTest(noConsent, seeds);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ThreePictureStory(bool noConsent)
        {
            var seeds = StoryDtoProvider.CreateFromStoryDtos(StoryDtoProvider.CreateStoryToterGrundWithAllBlockTypesAnd3Pictures());
            this.StoryUnderTest(noConsent, seeds);
        }

        private void StoryUnderTest(bool noConsent, StoryDtoProvider storyDtoProvider)
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsNotLoggedIn().Build())
            {
                foreach (var seed in storyDtoProvider.All)
                {
                    Story story = seed.ToStoryWithoutBlocks();
                    host.TrailContext.Stories.Add(story);
                }
                host.TrailContext.SaveChanges();

                foreach (var seed in storyDtoProvider.All)
                {
                    var response = host.GetFromServer($"{RouteName.StoryIndex}/{seed.ID}?noconsent={noConsent.ToString().ToLowerInvariant()}");
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}
