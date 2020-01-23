using System.Collections.Generic;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class BlobsBackendIndexViewModel : DatapagerViewModel
    {
        public List<Blob> Blobs { get; set; }
    }
}
