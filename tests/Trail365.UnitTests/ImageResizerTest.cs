using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Trail365.Graphics;
using Trail365.Seeds;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class ImageResizerTest
    {
        private static string GetTargetFileName(string sourceFileName, string prefix)
        {
            string folder = Directory.GetCurrentDirectory();
            string fn = Path.GetFileNameWithoutExtension(sourceFileName);
            string ext = Path.GetExtension(sourceFileName);

            return Path.Combine(folder, $"{prefix}{fn}{ext}");
        }

        public static IEnumerable<object[]> GetSampleFilenames()
        {
            List<object[]> list = new List<object[]>();
            foreach (int squarelength in new int[] { 110, 220, 500, 640, 800 })
            {
                list.AddRange(Images.All.Select(i => new object[] { i, squarelength }).ToArray());
            }
            return list;
        }

        [Theory]
        [MemberData(nameof(GetSampleFilenames))]
        public void ResizeFileTo120x120(string fileName, int squareLength)
        {
            var size = new Size(squareLength, squareLength);
            ImageResizer.Resize(fileName, GetTargetFileName(fileName, $"{squareLength}x{squareLength}_"), size);
        }
    }
}
