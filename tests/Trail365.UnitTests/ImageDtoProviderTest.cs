using Trail365.Seeds;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class ImageDtoProviderTest
    {
        [Fact]
        public void ShouldBeIsolated()
        {
            var i1 = ImageDtoProvider.CreateInstance();
            var i2 = ImageDtoProvider.CreateInstance();
            Assert.NotEqual(i1, i2);

            //Assert the isolation usecase!
            Assert.NotEqual(i1.Kahlenberg, i2.Kahlenberg);
            Assert.NotEqual(i1.Lindkogel, i2.Lindkogel);
        }
    }
}
