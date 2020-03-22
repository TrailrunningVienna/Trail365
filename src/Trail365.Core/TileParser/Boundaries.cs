using System;
using System.Diagnostics;
using System.Linq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.Internal;

namespace TrackExplorer.Core
{
    public class Boundaries
    {
        /// <summary>
        /// erzeugt einen Linestring, rechteckig an den Grenzen und an den Mittelpunkt!
        /// </summary>
        /// <returns></returns>
        public FeatureCollection CreateLineString()
        {
            FeatureCollection featureCollection = new FeatureCollection();
            GeometryFactory f = new GeometryFactory();
            //1. zeichne Rechteck aussen rum!
            LineString ls = f.CreateLineString(this.Envelope.Coordinates) as LineString;
            Feature f1 = new Feature(ls, null);
            featureCollection.Add(f1);
            var center = this.InteriorPoint.Coordinates.Single();
            //2 zeichne eine Linie von jedem Eck zum Mittelpunkt des Rechtecks
            foreach (var p in this.Envelope.Coordinates)
            {
                if (p == this.Envelope.Coordinates.Last())
                {
                    continue;
                }
                LineString halfdiag = f.CreateLineString(new Coordinate[] { p, center }) as LineString;
                featureCollection.Add(new Feature(halfdiag, null));
            }
            return featureCollection;
        }

        public Geometry Envelope { get; set; }
        public Point InteriorPoint { get; set; }

        private const int ZoomFallback = 12;

        private static int GetZoomForDist(double dist, bool isYAxis)
        {
            if (double.IsNaN(dist)) return ZoomFallback;
            if (double.IsInfinity(dist)) return ZoomFallback;
            dist = dist * 100000;
            int level = 16;

            if (dist > 5)
            {
                level = 15;
            }

            if (dist > 10)
            {
                level = 14;
            }

            if (dist > 100)
            {
                level = 14;
            }
            if (dist > 5000)
            {
                level = 13;
            }

            if (dist > 10000)
            {
                level = 12;
            }

            if (dist > 12000)
            {
                level = 11;
            }

            if (dist > 25000)
            {
                level = 10;
            }

            Debug.WriteLine($"Distance={dist} => ZoomLevel {level}");

            return level; ;
        }

        private static int FixProposedZoomLevel(int proposedZoomLevel, System.Drawing.Size size)
        {
            if (size.IsEmpty)
            {
                return proposedZoomLevel;
            }
            if (proposedZoomLevel <= 0) return proposedZoomLevel;
            //original level probably developed with 2560x1440

            int shortSide = Math.Min(size.Width, size.Height);
            int toSubtract = 0;

            if (shortSide <= 800)
            {
                toSubtract = 1;
            }

            if (shortSide <= 300)
            {
                toSubtract = 2;
            }
            return proposedZoomLevel - toSubtract;
        }

        public bool TryGetZoomLevel(out int zoomLevel, System.Drawing.Size size)
        {
            zoomLevel = -1;
            if (this.Envelope == null) return false;
            if (this.Envelope.IsEmpty) return false;
            if (this.InteriorPoint == null) return false;
            Guard.Assert(this.Envelope.IsValid);
            Guard.Assert(this.Envelope.IsSimple);
            Guard.Assert(this.Envelope.IsEmpty == false);

            if (this.Envelope.IsRectangle)
            {
                Coordinate last = null;
                double mX = 0.00000;
                double mY = 0.00000;

                foreach (var g in this.Envelope.Coordinates)
                {
                    if (last != null)
                    {
                        double d = Math.Abs(last.X - g.X);
                        mX = Math.Max(mX, d);
                        d = Math.Abs(last.Y - g.Y);
                        mY = Math.Max(mY, d);
                    }
                    last = g;
                }
                //WM 12.10.2019 we should add some "border" to the calculation!
                double borderFactor = 1.10; //%
                int zoomX = GetZoomForDist(mX * borderFactor, false);
                int zoomY = GetZoomForDist(mY * borderFactor, true);

                var proposedZoomLevel = Math.Min(zoomX, zoomY);
                zoomLevel = FixProposedZoomLevel(proposedZoomLevel, size);
            }
            else
            {
                throw new NotImplementedException($"Envelope.IsRecatnge==false");
            }

            return (zoomLevel > 0) && (zoomLevel < 18);
        }
    }
}
