using Trail365.Services;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class StartupTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public StartupTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void StartWithNullBlobService()
        {
            using (var host = TestHostBuilder.Empty().UseTestOutputHelper(OutputHelper).Build())
            {
                NullBlobService nbs = host.BlobService as NullBlobService;
                Assert.NotNull(nbs);
            }
        }

        [Fact]
        public void StartWithFileSystemBlobService()
        {
            using (var host = TestHostBuilder.Empty().UseFileSystemStorage().UseTestOutputHelper(OutputHelper).Build())
            {
                FileSystemBlobService fsbs = host.BlobService as FileSystemBlobService;
                Assert.NotNull(fsbs);
            }
        }
    }
}
