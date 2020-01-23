using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class TestHostBuilderTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public TestHostBuilderTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void ShouldCreateEmptyHost()
        {
            using (var host = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).Build())
            {
                host.Logger.LogTrace("Hello");
            }
        }

        [Fact]
        public void ShouldCreateHostWithIdentityContext()
        {
            using (var host = TestHostBuilder.Empty().WithIdentityContext().UseTestOutputHelper(OutputHelper).Build())
            {
                Assert.Empty(host.IdentityContext.Identities.ToList());
                Assert.Empty(host.IdentityContext.Users.ToList());
            }
        }
    }
}
