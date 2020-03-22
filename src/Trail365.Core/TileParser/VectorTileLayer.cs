using System.Collections.Generic;

namespace TrackExplorer.Core.TileParser
{
	public class VectorTileLayer
	{
        public VectorTileLayer()
        {
            VectorTileFeatures = new List<VectorTileFeature>();
        }

		public List<VectorTileFeature> VectorTileFeatures { get;set; }
		public string Name { get; set; }
		public uint Version { get; set; }
		public uint Extent { get; set; }
	}
}

