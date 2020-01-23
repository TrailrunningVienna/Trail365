using System.Collections.Generic;
using System.IO;
using System.Linq;
using Trail365.Graphics;
using Trail365.Seeds;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class ImageAnalyzerTest
    {
        public static IEnumerable<object[]> GetSampleFilenames()
        {
            return Images.All.Select(i => new object[] { i }).ToArray();
        }

        [Theory]
        [MemberData(nameof(GetSampleFilenames))]
        public void Should_Get_Size_and_MimeType(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                ImageAnalyzer.GetSize(stream, out string mimeType);
                Assert.NotEmpty(mimeType);
            }
        }

        [Theory]
        [MemberData(nameof(GetSampleFilenames))]
        public void Should_Get_MimeType(string fileName)
        {
            Assert.NotEmpty(ImageAnalyzer.GetMimeType(fileName));
        }
    }
}
