using System;
using System.Linq;
using Trail365.DTOs;
using Trail365.Seeds;
using Trail365.UnitTests.TestContext;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Api
{
    [Trait("Category", "BuildVerification")]
    public class EventApiControllerTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public EventApiControllerTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void ShouldReImportExternalEvents()
        {
            EventDto sample = new EventDto
            {
                Name = "demo",
                ExternalID = "exID",
                ExternalSource = "exsource",
                CreatedUtc = DateTime.UtcNow.AddDays(-1),
            };

            using (var host = TestHostBuilder.Empty().UseTestOutputHelper(this.OutputHelper).WithTrailContext().Build())
            {
                var hc = host.CreateEventsController();
                var dtoResult = hc.PostEvent(sample).GetAwaiter().GetResult().ToModel();

                Assert.Equal(dtoResult.ExternalSource, sample.ExternalSource);
                Assert.Equal(dtoResult.ExternalID, sample.ExternalID);
                Assert.Equal(dtoResult.CreatedUtc, sample.CreatedUtc);
                Assert.Equal(dtoResult.ModifiedUtc, sample.ModifiedUtc);
                var allevents = hc.GetEvents().GetAwaiter().GetResult().ToModel().ToList();
                var single = allevents.Where(ee => ee.ExternalSource == sample.ExternalSource && ee.ExternalID == sample.ExternalID).Single();
            }
        }

        [Fact]
        public void ShouldCreateListAndDeleteEvents()
        {
            var testData = EventDtoProvider.CreateDummyForPublicSeeds(5);

            using (var host = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).WithTrailContext().Build())
            {
                var hc = host.CreateEventsController();
                foreach (var eventDto in testData.All)
                {
                    var dtoResult = hc.PostEvent(eventDto).GetAwaiter().GetResult().ToModel();
                    Assert.Equal(dtoResult.ExternalSource, eventDto.ExternalSource);
                    Assert.Equal(dtoResult.ExternalID, eventDto.ExternalID);
                }
                Assert.Equal(testData.All.Length, host.TrailContext.Events.ToList().Count);

                var results = hc.GetEvents().GetAwaiter().GetResult().Value.ToArray();
                foreach (var evDto in results)
                {
                    hc.DeleteEvent(evDto.ID).GetAwaiter().GetResult();
                }
                Assert.Empty(host.TrailContext.Events.ToList());
            }
        }
    }
}
