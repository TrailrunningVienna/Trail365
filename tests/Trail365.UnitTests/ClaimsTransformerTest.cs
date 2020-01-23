using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Trail365.Web;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class ClaimsTransformerTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public ClaimsTransformerTest(ITestOutputHelper outputHelper)
        {
            this.OutputHelper = outputHelper;
        }

        //private static readonly string ExternalNameIdentifier = "ExternalUserID0814_4711";

        [Fact]
        public async Task FirstContactWithClaims()
        {
            //string authType = "google";
            using (var host = TestHostBuilder.Empty().WithIdentityContext().UseTestOutputHelper(this.OutputHelper).Build())
            {
                ClaimsTransformer transformer = new ClaimsTransformer(host.IdentityContext, host.SettingsMonitor, host.LoggerFactory.CreateLogger<ClaimsTransformer>());
                //IClaimsTransformation transformation = transformer;
                //Claim[] claims = new Claim[] { new Claim(ClaimTypes.NameIdentifier, ExternalNameIdentifier), new Claim(ClaimTypes.Email, "donald@trump@white.house") };
                //ClaimsIdentity identity = new ClaimsIdentity(claims, authType);
                //Assert.True(identity.IsAuthenticated);
                //ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                //var transformedPrincipal = await transformation.TransformAsync(principal);

                //Assert.False(transformedPrincipal.Identity.IsAuthenticated);
                //FederatedIdentity newIdentity = host.IdentityContext.CreateIdentity(principal);
                //Assert.Null(newIdentity.User);// we expect that the user is NEVER included/resolved here, for perf
                //Assert.NotEqual(System.Guid.Empty, newIdentity.UserID);

                //var i = host.IdentityContext.SaveChanges();
                //Assert.True(i > 1);
                //transformedPrincipal = await transformation.TransformAsync(principal);
                //Assert.True(transformedPrincipal.Identity.IsAuthenticated);
                //var cl = transformedPrincipal.FindAll(ClaimTypes.PrimarySid).Single();
                //Guid primarySID = Guid.Parse(cl.Value);
                //Assert.Equal(newIdentity.UserID, primarySID);
                await host.IdentityContext.SaveChangesAsync();
            }
        }
    }
}
