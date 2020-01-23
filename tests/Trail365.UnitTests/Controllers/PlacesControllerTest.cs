using Trail365.Entities;
using Trail365.UnitTests.TestContext;
using Trail365.ViewModels;
using Xunit;

namespace Trail365.UnitTests.Controllers
{
    [Trait("Category", "BuildVerification")]
    public class PlacesControllerTest
    {
        [Theory]
        [InlineData(null, null, null, null, 4)]
        [InlineData("IT", null, null, null, 1)]
        [InlineData(null, "IT", null, null, 1)]
        [InlineData(null, "it", null, null, 1)]
        [InlineData(null, "AME AT", null, null, 1)]
        [InlineData(null, null, "EXTSOURCE-DE", null, 1)]
        [InlineData(null, null, null, "EXTID-FR", 1)]
        public void ShouldIndexForSearch(string country, string searchText, string source, string sourceID, int expectedResults)
        {
            using (var host = TestHostBuilder.DefaultForBackend().Build())
            {
                //seeding
                foreach (string cnt in new string[] { "AT", "IT", "DE", "FR" })
                {
                    var place = new Place
                    {
                        Name = "NAME-" + cnt,
                        CountryTwoLetterISOCode = cnt,
                        Country = "COUNTRY-" + cnt,
                        ExternalSource = "EXTSOURCE-" + cnt,
                        ExternalID = "EXTID-" + cnt
                    };
                    host.TrailContext.Places.Add(place);
                }

                host.TrailContext.SaveChanges();

                var controller = host.CreateBackendPlacesController();
                var input = new PlacesBackendIndexViewModel
                {
                    SearchText = searchText,
                    ExternalID = sourceID,
                    ExternalSource = source,
                    CountryTwoLetterISOCode = country,
                };
                var result = controller.Index(input).ToModel<PlacesBackendIndexViewModel>();
                Assert.Equal(expectedResults, result.Places.Count);
            }
        }
    }
}
