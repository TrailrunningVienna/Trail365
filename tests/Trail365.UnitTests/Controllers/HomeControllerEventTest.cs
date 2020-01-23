using Trail365.Entities;
using Trail365.UnitTests.TestContext;
using Trail365.ViewModels;
using Xunit;

namespace Trail365.UnitTests.Controllers
{
    [Trait("Category", "BuildVerification")]
    public class HomeControllerEventTest
    {
        [Fact]
        public void ShouldCheckEventAndTrailListAccess()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsUser().Build())
            {
                //Arrange: provide one Event with one Track, Event public, Track only for admins
                Event eventItem = new Event
                {
                    ListAccess = AccessLevel.Public,
                    Name = "xyz"
                };

                Trail trailItem = new Trail
                {
                    ListAccess = AccessLevel.Administrator,
                    GpxDownloadAccess = AccessLevel.Administrator,
                    Name = "abcd"
                };

                eventItem.Trail = trailItem;
                eventItem.TrailID = trailItem.ID;

                host.TrailContext.Events.Add(eventItem);
                host.TrailContext.Trails.Add(trailItem);

                host.TrailContext.SaveChanges();

                using (var hc = host.CreateHomeController())
                {
                    //after AUTH as user
                    //hc.AuthenticateAs("demo@domain.com", Core.Entities.Role.User);
                    var action = hc.Event(new EventRequestViewModel { ID = eventItem.ID });
                    var result = action.ToModel<EventViewModel>();
                    Assert.Null(result.Trail);

                    trailItem.ListAccess = AccessLevel.Public;
                    host.TrailContext.Trails.Update(trailItem);
                    host.TrailContext.SaveChanges();

                    action = hc.Event(new EventRequestViewModel { ID = eventItem.ID });
                    result = action.ToModel<EventViewModel>();
                    Assert.NotNull(result.Trail);
                }
            }
        }
    }
}
