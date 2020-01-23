using Trail365.UnitTests.Properties;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class AzureAppServiceDeploymentStatusTest
    {
        [Fact]
        public void ShouldReadFromXml()
        {
            var status = AzureAppServiceDeploymentStatus.ReadFromString(TestResource.DeploymentStatusXml);
            Assert.NotNull(status);
        }
    }
}
