using System.Collections.Generic;
using System.Text;
using Trail365.Entities;

namespace Trail365.Services
{
    public class TrailImporterTaskResult
    {
        public List<Trail> UpdatedTrails { get; private set; } = new List<Trail>();
        public List<Trail> InsertedTrails { get; private set; } = new List<Trail>();
        public StringBuilder Log { get; set; } = new StringBuilder();
        public int Changes { get; set; } = 0;

        public List<Blob> UpdatedBlobs { get; set; } = new List<Blob>();

        public List<Blob> InsertedBlobs { get; set; } = new List<Blob>();
    }
}
