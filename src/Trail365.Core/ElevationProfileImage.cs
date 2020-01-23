using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.IO;
using Trail365.Graphics;
using Trail365.Internal;

namespace Trail365
{
    public class ElevationProfileImage
    {
        public static byte[] CreateImageAsPng(string xml, ElevationProfileDiagram diagram, Size imageSize)
        {
            var success = TryCreateImageAsPng(xml, diagram, imageSize, NullLogger.Instance, out var data);
            if (success == false) throw new InvalidOperationException("CreateImageAsPng failed");
            return data;
        }

        public static bool TryGetElevationProfileData(string xml, out ElevationProfileData profileData)
        {
            GpxFile file = TrailExtender.ReadGpxFileAndBOM(xml);
            return TryGetElevationProfileData(file, out profileData);
        }

        public static bool TryGetElevationProfileData(GpxFile file, out ElevationProfileData profileData)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            profileData = null;
            var track = file.Tracks.FirstOrDefault();

            if (track == null)
            {
                return false;
            }

            var segment = track.Segments.FirstOrDefault();

            if (segment == null)
            {
                return false;
            }
            if (segment.Waypoints == null)
            {
                return false;
            }

            if (ElevationProfileData.TryGetElevationProfileData(segment.Waypoints, out profileData) == false)
            {
                return false;
            }
            Guard.AssertNotNull(profileData);
            return true;
        }

        public static bool TryCreateImageAsPng(ElevationProfileData elevationData, ElevationProfileDiagram diagram, Size imageSize, ILogger logger, out byte[] imageData)
        {
            if (elevationData == null) throw new ArgumentNullException(nameof(elevationData));
            if (diagram == null) throw new ArgumentNullException(nameof(diagram));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            Stopwatch sw3 = Stopwatch.StartNew();
            var line = CreateLine(elevationData, diagram, imageSize);
            sw3.Stop();
            Stopwatch sw4 = Stopwatch.StartNew();
            var result = ImageFactory.CreateLineImageAsPng(line, imageSize);
            sw4.Stop();
            //WM 10/2019 sw4 is the most critical (longest)
            logger.LogTrace($"{nameof(TryCreateImageAsPng)}: CreateLineDuration={sw3.ElapsedMilliseconds}ms, CreateLineImageDuration={sw4.ElapsedMilliseconds}ms (Line.Length={line.Length})");
            imageData = result;
            return true;
        }

        public static bool TryCreateImageAsPng(string xml, ElevationProfileDiagram diagram, Size imageSize, ILogger logger, out byte[] imageData)
        {
            if (string.IsNullOrEmpty(xml)) throw new ArgumentNullException(nameof(xml));
            if (diagram == null) throw new ArgumentNullException(nameof(diagram));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            Stopwatch sw1 = Stopwatch.StartNew();
            GpxFile file = TrailExtender.ReadGpxFileAndBOM(xml);
            sw1.Stop();

            var track = file.Tracks.FirstOrDefault();

            if (track == null)
            {
                throw new NotImplementedException("NI1");
                //return target;
            }
            var segment = track.Segments.FirstOrDefault();

            if (segment == null)
            {
                throw new NotImplementedException("NI2");
            }
            if (segment.Waypoints == null)
            {
                throw new NotImplementedException("NI3");
            }

            Stopwatch sw2 = Stopwatch.StartNew();

            if (ElevationProfileData.TryGetElevationProfileData(segment.Waypoints, out var details) == false)
            {
                logger.LogDebug($"{nameof(TryCreateImageAsPng)}: No elevation profile data inside the waypoints");
                imageData = null;
                return false;
            }
            sw2.Stop();
            return TryCreateImageAsPng(details, diagram, imageSize, logger, out imageData);
        }

        public static PointF[] CreateLine(ElevationProfileData profile, ElevationProfileDiagram diagram, Size canvasSize)
        {
            if (diagram == null) throw new ArgumentNullException(nameof(diagram));
            if (canvasSize.IsEmpty) throw new ArgumentNullException("canvasSize cannot be empty");

            float destWidthBmpAsFloat = Convert.ToSingle(canvasSize.Width);
            float destHeightBmpAsFloat = Convert.ToSingle(canvasSize.Height);

            float height = profile.MaxAltitude - profile.MinAltitude;
            Guard.Assert(height > 0);

            var factors = diagram.GetFactors(canvasSize, profile.Distance, height);
            var xFactor = factors.Item1;
            var yFactor = factors.Item2;

            List<PointF> drawingLine = new List<PointF>();
            float altitudeFix = 0; //in case of altitude-delta usage, we should ensure that the unused parts of the y-axis are half on top and half on bottom

            if (diagram.AltitudeDelta.HasValue)
            {
                float altitudeUsage = profile.MaxAltitude - profile.MinAltitude;
                float remainingaltitude = diagram.AltitudeDelta.Value - altitudeUsage;
                altitudeFix = remainingaltitude / 2;
            }

            foreach (var pt in profile.Line)
            {
                var altitude = (pt.Y - profile.MinAltitude) + altitudeFix;

                var reverseY = (altitude * yFactor);
                var y = (destHeightBmpAsFloat - reverseY);
                if (y < 0)
                {
                    continue; //outside
                }
                if (y > destHeightBmpAsFloat)
                {
                    continue; //outside, ignore!
                }
                var x = pt.X * xFactor;
                Guard.Assert(x >= 0);
                if (x > destWidthBmpAsFloat)
                {
                    continue; //outside, ignore
                }
                var newPoint = new PointF(x, y);
                drawingLine.Add(newPoint);
            }
            return drawingLine.ToArray();
        }
    }
}
