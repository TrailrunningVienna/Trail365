using System.IO;
using System.Linq;
using Trail365.Entities;
using Trail365.Seeds;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class TrailExtenderTest
    {
        [Fact]
        public void ShouldSplitGpxFile()
        {
            string gpxTrack = File.ReadAllText(GpxTracks.MultiFile3Sample);
            var splitterResult = TrailExtender.GpxTrackSplitter(gpxTrack).ToList();
            Assert.Equal(3, splitterResult.Count);
            foreach (string gpxAsXml in splitterResult)
            {
                var fi = TrailExtender.ReadGpxFileInfo(gpxAsXml, new Trail());
            }
        }

        [Fact]
        public void ShouldReturnValuesForU4U4Toiflhuette()
        {
            string gpxTrack = File.ReadAllText(GpxTracks.U4U4Toiflhuette);
            var result = TrailExtender.ReadGpxFileInfo(gpxTrack, new Trail());
            Assert.True(result.DistanceMeters > 24980);
            Assert.True(result.DistanceMeters < 25000);
            Assert.True(result.AscentMeters > 650);
            Assert.True(result.AscentMeters < 655);
            Assert.True(result.DescentMeters > 694);
            Assert.True(result.DescentMeters < 697);
            Assert.True(result.AltitudeAtStart == 206);
            Assert.True(result.MaximumAltitude == 482);
            Assert.True(result.MinimumAltitude == 166);
        }


        [Fact]
        public void ShouldReturnFeatureCollectionFromSharedTrack()
        {
            var result = TrailExtender.ConvertToFeatureCollection(GpxTracks.SharedRoute);
            Assert.Equal(1385, result[0].Geometry.Coordinates.Length);
        }

        [Fact]
        public void ShouldReturnValuesForSharedTrack()
        {
            string gpxTrack = File.ReadAllText(GpxTracks.SharedRoute);
            var result = TrailExtender.ReadGpxFileInfo(gpxTrack, new Trail());
            Assert.True(result.DistanceMeters > 37400);
            Assert.True(result.DistanceMeters < 37500);
            Assert.True(result.AscentMeters > 1460);
            Assert.True(result.AscentMeters < 1470);
            Assert.True(result.DescentMeters > 1465);
            Assert.True(result.DescentMeters < 1470);
            Assert.True(result.AltitudeAtStart == 439);
            Assert.True(result.MaximumAltitude == 597);
            Assert.True(result.MinimumAltitude == 330);
        }

        [Fact]
        public void ShouldReturnValuesForBuschberg()
        {
            string gpxTrack = File.ReadAllText(GpxTracks.Buschberg);
            var result = TrailExtender.ReadGpxFileInfo(gpxTrack, new Trail());
            Assert.True(result.DistanceMeters > 120000);
            Assert.True(result.DistanceMeters < 121000);
            Assert.True(result.AscentMeters > 1000);
            Assert.True(result.AscentMeters < 1100);
            Assert.True(result.DescentMeters > 1000);
            Assert.True(result.DescentMeters < 1100);

            Assert.True(result.AltitudeAtStart > 140);
            Assert.True(result.AltitudeAtStart < 150);
            Assert.True(result.MaximumAltitude == 410);
            Assert.True(result.MinimumAltitude < 150);
            Assert.True(result.MinimumAltitude > 130);
        }

        [Fact]
        public void ShouldReturnValuesForHusarenTempel()
        {
            string gpxTrack = File.ReadAllText(GpxTracks.HusarenTempel);
            var result = TrailExtender.ReadGpxFileInfo(gpxTrack, new Trail());
            Assert.True(result.DistanceMeters > 15200);
            Assert.True(result.DistanceMeters < 15300);
            Assert.True(result.AscentMeters.HasValue);
            Assert.True(result.DescentMeters.HasValue);
        }

        [Fact]
        public void ShouldReturnValuesForRosengarten()
        {
            string gpxTrack = File.ReadAllText(GpxTracks.Rosengarten);
            var result = TrailExtender.ReadGpxFileInfo(gpxTrack, new Trail());
            Assert.True(result.DistanceMeters > 43900);
            Assert.True(result.DistanceMeters < 44000);
            Assert.True(result.AscentMeters > 3000);
            Assert.True(result.AscentMeters < 3100);
            Assert.True(result.DescentMeters > 3100);
            Assert.True(result.DescentMeters < 3200);

            Assert.True(result.AltitudeAtStart > 1120);
            Assert.True(result.AltitudeAtStart < 1200);
            Assert.True(result.MaximumAltitude == 2610);
            Assert.True(result.MinimumAltitude < 1100);
            Assert.True(result.MinimumAltitude > 1000);
        }

        [Fact]
        public void ShouldReturnValuesVTRLight()
        {
            string gpxTrack = File.ReadAllText(GpxTracks.VTRLight);
            var result = TrailExtender.ReadGpxFileInfo(gpxTrack, new Trail());
            Assert.True(result.DistanceMeters > 6300);
            Assert.True(result.DistanceMeters < 6400);
            Assert.True(result.AscentMeters > 290);
            Assert.True(result.AscentMeters < 310);
            Assert.True(result.DescentMeters > 290);
            Assert.True(result.DescentMeters < 300);
        }

        [Fact]
        public void ShouldReturnValuesVTRClassic()
        {
            string gpxTrack = File.ReadAllText(GpxTracks.VTRClassic);
            var result = TrailExtender.ReadGpxFileInfo(gpxTrack, new Trail());
            Assert.True(result.DistanceMeters > 14100);
            Assert.True(result.DistanceMeters < 14200);
            Assert.True(result.AscentMeters > 610);
            Assert.True(result.AscentMeters < 615);
            Assert.True(result.DescentMeters > 600);
            Assert.True(result.DescentMeters < 610);
        }
    }
}
