using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Trail365.Seeds;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class ElevationProfileImageTest
    {
        public static IEnumerable<object[]> GetAltitudeAlignmentSampleData()
        {
            List<object[]> samples = new List<object[]>();

            ElevationProfileDiagram longDistanzeDiagram = new ElevationProfileDiagram
            {
                DistanceBase = 40000,
                AltitudeMinValue = 0,
                AltitudeMaxValue = 3500
            };

            ElevationProfileDiagram shortDistanceDiagram = new ElevationProfileDiagram
            {
                DistanceBase = 20000,
                AltitudeMinValue = 0,
                AltitudeMaxValue = 3500
            };

            ElevationProfileDiagram normalizedHightDiagramBig = new ElevationProfileDiagram
            {
                DistanceBase = 50000,

                //requirements for High: max-min = static value like distanceBase
                AltitudeDelta = 2000
            };

            ElevationProfileDiagram normalizedHightDiagramSmall = new ElevationProfileDiagram
            {
                DistanceBase = 25000,
                //requirements for High: max-min = static value like distanceBase
                AltitudeDelta = 1000
            };

            foreach (var size in new Size[] { new Size(400, 50), new Size(400, 200) })
            {
                foreach (var diagram in new ElevationProfileDiagram[] { normalizedHightDiagramSmall, normalizedHightDiagramBig, longDistanzeDiagram, shortDistanceDiagram })
                {
                    foreach (var filename in GpxTracks.AllValidTracks)
                    {
                        string fn = Path.GetFileNameWithoutExtension(filename).Replace(" ", string.Empty).Replace("-", string.Empty);
                        string sz = $"{size.Width}x{size.Height}";
                        string dist = $"{System.Convert.ToInt32(diagram.DistanceBase.Value / 1000)}km";
                        string hg = "0m3500m";
                        if (diagram.AltitudeDelta.HasValue)
                        {
                            hg = $"delta{System.Convert.ToInt32(diagram.AltitudeDelta.Value)}";
                        }
                        samples.Add(new object[] { File.ReadAllText(filename), size, diagram, $"{dist}_{sz}_{hg}_{fn}" });
                    }
                }
            }
            return samples.ToArray();
        }

        public static IEnumerable<object[]> GetSampleData()
        {
            List<object[]> samples = new List<object[]>();

            ElevationProfileDiagram defaultDiagram = new ElevationProfileDiagram();

            ElevationProfileDiagram longDistancediagram = new ElevationProfileDiagram
            {
                DistanceBase = 60000,
                AltitudeMinValue = 0,
                AltitudeMaxValue = 3000
            };

            ElevationProfileDiagram shortDistancediagram = new ElevationProfileDiagram
            {
                DistanceBase = 130000,
                AltitudeMinValue = 0,
                AltitudeMaxValue = 3000
            };

            ElevationProfileDiagram alignedDistancediagram = new ElevationProfileDiagram
            {
                DistanceBase = 40000,
                AltitudeMinValue = 0,
                AltitudeMaxValue = 3500
            };

            samples.Add(new object[] { File.ReadAllText(GpxTracks.Rosengarten), new Size(800, 600), defaultDiagram, "rg_d_600x600" });
            samples.Add(new object[] { File.ReadAllText(GpxTracks.Buschberg), new Size(800, 600), defaultDiagram, "bb_d_600x600" });

            samples.Add(new object[] { File.ReadAllText(GpxTracks.Rosengarten), new Size(800, 600), longDistancediagram, "rg_l_600x600" });
            samples.Add(new object[] { File.ReadAllText(GpxTracks.Rosengarten), new Size(400, 200), longDistancediagram, "rg_l_400x200" });
            samples.Add(new object[] { File.ReadAllText(GpxTracks.Buschberg), new Size(800, 600), longDistancediagram, "bb_l_600x600" });
            samples.Add(new object[] { File.ReadAllText(GpxTracks.Buschberg), new Size(400, 200), longDistancediagram, "bb_l_400x200" });

            samples.Add(new object[] { File.ReadAllText(GpxTracks.VTRClassic), new Size(400, 200), shortDistancediagram, "vtrc_s_400x200" });
            samples.Add(new object[] { File.ReadAllText(GpxTracks.VTRLight), new Size(400, 200), shortDistancediagram, "vtrl_s_400x200" });

            //elevation profile alignement samples
            Size defaultSize = new Size(400, 200);
            string sz = "400x200";

            samples.Add(new object[] { File.ReadAllText(GpxTracks.VTRClassic), defaultSize, alignedDistancediagram, $"vtrc_aligned_{sz}" });
            samples.Add(new object[] { File.ReadAllText(GpxTracks.VTRLight), defaultSize, alignedDistancediagram, $"vtrl_aligned_{sz}" });
            samples.Add(new object[] { File.ReadAllText(GpxTracks.U4U4Toiflhuette), defaultSize, alignedDistancediagram, $"toifl_aligned_{sz}" });
            samples.Add(new object[] { File.ReadAllText(GpxTracks.HusarenTempel), defaultSize, alignedDistancediagram, $"templ_aligned_{sz}" });
            samples.Add(new object[] { File.ReadAllText(GpxTracks.Rosengarten), defaultSize, alignedDistancediagram, $"rg_aligned_{sz}" });

            return samples;
        }

        [Theory]
        [MemberData(nameof(GetSampleData))]
        public void ShouldRenderDifferentSamples(string gpxXml, Size imageSize, ElevationProfileDiagram diagram, string name)
        {
            byte[] bytes = ElevationProfileImage.CreateImageAsPng(gpxXml, diagram, imageSize);
            File.WriteAllBytes(GetTargetFileNameAsPng(name), bytes);
        }

        [Theory]
        [MemberData(nameof(GetAltitudeAlignmentSampleData))]
        public void ShouldRenderAlignedSamples(string gpxXml, Size imageSize, ElevationProfileDiagram diagram, string name)
        {
            byte[] bytes = ElevationProfileImage.CreateImageAsPng(gpxXml, diagram, imageSize);
            File.WriteAllBytes(GetTargetFileNameAsPng(name, "aligned"), bytes);
        }

        private static string GetTargetFileNameAsPng(string name, string subPath = null)
        {
            string folder = Directory.GetCurrentDirectory();

            if (string.IsNullOrEmpty(subPath) == false)
            {
                folder = Path.Combine(folder, subPath);
            }
            if (Directory.Exists(folder) == false)
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, $"{name}.png");
        }

        [Fact]
        public void ShouldDrawProfile_Toiflhuette()
        {
            string gpxTrack = File.ReadAllText(GpxTracks.U4U4Toiflhuette);
            ElevationProfileDiagram template = new ElevationProfileDiagram();
            var data = ElevationProfileImage.CreateImageAsPng(gpxTrack, template, new System.Drawing.Size(1024, 1024));
            File.WriteAllBytes(GetTargetFileNameAsPng("toifl_elevation"), data);
        }

        [Fact]
        public void ShouldDrawProfile_VTRLight()
        {
            string gpxTrack = File.ReadAllText(GpxTracks.VTRLight);
            ElevationProfileDiagram template = new ElevationProfileDiagram();
            var data = ElevationProfileImage.CreateImageAsPng(gpxTrack, template, new System.Drawing.Size(1024, 1024));
            File.WriteAllBytes(GetTargetFileNameAsPng("vtrlight_elevation"), data);
        }

        [Fact]
        public void ShouldDrawProfile_Rosengarten()
        {
            string gpxTrack = File.ReadAllText(GpxTracks.Rosengarten);
            ElevationProfileDiagram template = new ElevationProfileDiagram();
            var data = ElevationProfileImage.CreateImageAsPng(gpxTrack, template, new System.Drawing.Size(1024, 1024));
            File.WriteAllBytes(GetTargetFileNameAsPng("rosengarten_elevation"), data);
        }

        [Fact]
        public void ShouldDrawProfile_VTRClassic()
        {
            string gpxTrack = File.ReadAllText(GpxTracks.VTRClassic);
            ElevationProfileDiagram template = new ElevationProfileDiagram();
            var data = ElevationProfileImage.CreateImageAsPng(gpxTrack, template, new System.Drawing.Size(1024, 1024));
            File.WriteAllBytes(GetTargetFileNameAsPng("vtrclassic_elevation"), data);
        }
    }
}
