using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Trail365.TileParser.Contract;

namespace Trail365.TileParser
{

    public static class VectorTileFeatureExtensions
    {
        private static List<Coordinate> Project(List<VectorTileCoordinate> coords, int x, int y, int z, uint extent)
        {
            return coords.Select(coord => coord.ToCoordinate(x, y, z, extent)).ToList();
        }

        private static List<Coordinate> Project(List<VectorTileCoordinate> coords, TileInfo map, uint extent)
        {
            return coords.Select(coord => coord.ToCoordinate(map, extent)).ToList();
        }

        private static LineString CreateLineString(GeometryFactory factory, List<Coordinate> pos)
        {
            return factory.CreateLineString(pos.ToArray());
            //return new LineString(pos);
        }


        private static Geometry GetPointGeometry(GeometryFactory factory, List<Coordinate> pointList)
        {
            Geometry geom;
            if (pointList.Count == 1)
            {
                geom = factory.CreatePoint(pointList[0]);
            }
            else
            {
                //var pnts = pointList.Select(p => new Point(p)).ToList();
                geom = factory.CreateMultiPointFromCoords(pointList.ToArray());
            }
            return geom;
        }

        private static List<LineString> GetLineStringList(GeometryFactory factory, List<List<Coordinate>> pointList)
        {
            return pointList.Select(part => CreateLineString(factory, part)).ToList();
        }

        private static Geometry GetLineGeometry(GeometryFactory factory, List<List<Coordinate>> pointList)
        {
            Geometry geom;

            if (pointList.Count == 1)
            {
                geom = factory.CreateLineString(pointList[0].ToArray());
            }
            else
            {
                geom = new MultiLineString(GetLineStringList(factory, pointList).ToArray());
            }
            return geom;
        }

        private static Polygon GetPolygon(GeometryFactory factory, List<List<Coordinate>> lines)
        {
            var res = new List<LineString>();
            foreach (var innerring in lines)
            {
                var line = CreateLineString(factory, innerring);//  new LineString(innerring);
                throw new NotImplementedException("NI");
                //if (line.IsLinearRing() && line.IsClosed())
                //{
                //    res.Add(line);
                //}
            }
            throw new NotImplementedException("NI2");
            //var geom =  new Polygon(res);
            //return geom;
        }

        private static Geometry GetPolygonGeometry(GeometryFactory factory, List<List<List<Coordinate>>> polygons)
        {
            {
                Geometry geom = null;

                if (polygons.Count == 1)
                {
                    geom = GetPolygon(factory, polygons[0]);
                }

                else if (polygons.Count > 0)
                {
                    var multipolys = new List<Polygon>();
                    foreach (var poly in polygons)
                    {
                        var pl = GetPolygon(factory, poly);
                        multipolys.Add(pl);
                    }
                    throw new NotImplementedException("NI3");
                    //geom = new MultiPolygon(multipolys);
                }
                return geom;
            }
        }

        public static List<Coordinate> ProjectPoints(List<List<VectorTileCoordinate>> Geometry, int x, int y, int z, uint extent)
        {
            var projectedCoords = new List<Coordinate>();
            var coords = new List<VectorTileCoordinate>();

            foreach (var g in Geometry)
            {
                coords.Add(g[0]);
                projectedCoords = Project(coords, x, y, z, extent);
            }
            return projectedCoords;
        }

        public static List<Coordinate> ProjectPoints(List<List<VectorTileCoordinate>> Geometry, TileInfo map, uint extent)
        {
            var projectedCoords = new List<Coordinate>();
            var coords = new List<VectorTileCoordinate>();

            foreach (var g in Geometry)
            {
                coords.Add(g[0]);
                projectedCoords = Project(coords, map, extent);
            }
            return projectedCoords;
        }

        public static List<List<Coordinate>> ProjectLines(List<List<VectorTileCoordinate>> Geometry, TileInfo map, uint extent)
        {
            var projectedCoords = new List<Coordinate>();
            var pointList = new List<List<Coordinate>>();
            foreach (var g in Geometry)
            {
                projectedCoords = Project(g, map, extent);
                pointList.Add(projectedCoords);
            }
            return pointList;
        }


        public static List<List<Coordinate>> ProjectLines(List<List<VectorTileCoordinate>> Geometry, int x, int y, int z, uint extent)
        {
            var projectedCoords = new List<Coordinate>();
            var pointList = new List<List<Coordinate>>();
            foreach (var g in Geometry)
            {
                projectedCoords = Project(g, x, y, z, extent);
                pointList.Add(projectedCoords);
            }
            return pointList;
        }

        public static List<List<List<Coordinate>>> ProjectPolygons(List<List<List<VectorTileCoordinate>>> Geometry, TileInfo map, uint extent)
        {
            var projectedCoords = new List<List<Coordinate>>();
            var result = new List<List<List<Coordinate>>>();
            foreach (var g in Geometry)
            {
                projectedCoords = ProjectLines(g, map, extent);
                result.Add(projectedCoords);
            }
            return result;
        }


        public static List<List<List<Coordinate>>> ProjectPolygons(List<List<List<VectorTileCoordinate>>> Geometry, int x, int y, int z, uint extent)
        {
            var projectedCoords = new List<List<Coordinate>>();
            var result = new List<List<List<Coordinate>>>();
            foreach (var g in Geometry)
            {
                projectedCoords = ProjectLines(g, x, y, z, extent);
                result.Add(projectedCoords);
            }
            return result;
        }


        public static Feature ToGeoJSON(this VectorTileFeature vectortileFeature, TileInfo map)
        {
            GeometryFactory f = new GeometryFactory();
            Geometry geom = null;
            switch (vectortileFeature.GeometryType)
            {
                case TileGeometryType.Point:
                    var projectedPoints = ProjectPoints(vectortileFeature.Geometry, map, vectortileFeature.Extent);
                    geom = GetPointGeometry(f, projectedPoints);
                    break;
                case TileGeometryType.LineString:
                    var projectedLines = ProjectLines(vectortileFeature.Geometry, map, vectortileFeature.Extent);
                    geom = GetLineGeometry(f, projectedLines);
                    break;
                case TileGeometryType.Polygon:
                    var rings = ClassifyRings.Classify(vectortileFeature.Geometry);
                    var projectedPolygons = ProjectPolygons(rings, map, vectortileFeature.Extent);
                    geom = GetPolygonGeometry(f, projectedPolygons);
                    break;
            }

            AttributesTable attribs = new AttributesTable();
            //id: vectortileFeature.Id;

            var result = new Feature(geom, attribs);

            // add attributes
            foreach (var item in vectortileFeature.Attributes)
            {
                attribs.Add(item.Key, item.Value);

            }
            return result;
        }


        public static Feature ToGeoJSON(this VectorTileFeature vectortileFeature, int x, int y, int z)
        {
            return ToGeoJSON(vectortileFeature, TileInfo.CreateTileInfoFromCoordinate(x, y, z));
        }
    }
}
