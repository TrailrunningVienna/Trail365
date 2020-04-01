using System.Collections.Generic;

namespace Trail365.TileParser
{
	public class VectorTileFeature
	{
        public string Id { get; set; }
		public List<List<VectorTileCoordinate>> Geometry {get;set;}
		public List<KeyValuePair<string, object>> Attributes { get; set; }
		public Tile.GeomType GeometryType { get; set; }
        public uint Extent { get; set; }
    }
}

