using System;
using System.Collections.Generic;
using System.Linq;
using Trail365.DTOs;
using Trail365.Seeds;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Api
{
    [Trait("Category", "Routing")]
    [Trait("Category", "BuildVerification")]
    public class EventRouteTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public EventRouteTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Fact]
        public void ShouldCreateEventsViaDto()
        {
            var testData = EventDtoProvider.CreateFromEventDtos(EventDtoProvider.CreateEventWithExternalSource());

            using (var server = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).WithTrailContext().UseStaticAuthenticationAsAdmin().Build())
            {
                foreach (var seed in testData.All)
                {
                    using (var client = server.CreateClient())
                    {
                        client.PostEventViaApi(seed).EnsureSuccessStatusCode();
                    }
                }
                Assert.Equal(testData.All.Length, server.TrailContext.Events.ToList().Count);
                var response = server.GetFromServer(RouteName.EventsApi).EnsureSuccessStatusCode();

                string resultJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var events = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EventDto>>(resultJson);
                Assert.Equal(events.Count, testData.All.Length);
            }
        }
    }
}
