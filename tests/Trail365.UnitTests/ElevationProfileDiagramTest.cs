using System.Drawing;
using System.IO;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class ElevationProfileDiagramTest
    {
        private static string GetTargetFileNameAsPng(string name)
        {
            string folder = Directory.GetCurrentDirectory();
            return Path.Combine(folder, $"{name}.png");
        }

        [Fact]
        public void ShouldCalculateFactors_x0_5_without_baselineData()
        {
            ElevationProfileDiagram diagram = new ElevationProfileDiagram
            {
                DistanceBase = null,
                AltitudeMinValue = null,
                AltitudeMaxValue = null
            };
            var size = new Size(5000, 1000);

            var factors = diagram.GetFactors(size, 10000, 2000);
            Assert.Equal(0.5, factors.Item1);
            Assert.Equal(0.5, factors.Item2);
        }

        [Fact]
        public void ShouldCalculateFactors_x1_without_baselineData()
        {
            ElevationProfileDiagram diagram = new ElevationProfileDiagram
            {
                DistanceBase = null,
                AltitudeMinValue = null,
                AltitudeMaxValue = null
            };
            var size = new Size(5000, 1000);

            var factors = diagram.GetFactors(size, 5000, 1000);
            Assert.Equal(1, factors.Item1);
            Assert.Equal(1, factors.Item2);
        }

        [Fact]
        public void ShouldCalculateFactors_x1_baselineData()
        {
            ElevationProfileDiagram diagram = new ElevationProfileDiagram
            {
                DistanceBase = 5000,
                AltitudeMinValue = 0,
                AltitudeMaxValue = 1000
            };
            var size = new Size(5000, 1000);

            var factors = diagram.GetFactors(size, null, null);
            Assert.Equal(1, factors.Item1);
            Assert.Equal(1, factors.Item2);
        }

        [Fact]
        public void ShouldCalculateFactors_x0_5_baselineData()
        {
            ElevationProfileDiagram diagram = new ElevationProfileDiagram
            {
                DistanceBase = 10000,
                AltitudeMinValue = 0,
                AltitudeMaxValue = 2000
            };
            var size = new Size(5000, 1000);

            var factors = diagram.GetFactors(size, null, null);
            Assert.Equal(0.5, factors.Item1);
            Assert.Equal(0.5, factors.Item2);
        }
    }
}
