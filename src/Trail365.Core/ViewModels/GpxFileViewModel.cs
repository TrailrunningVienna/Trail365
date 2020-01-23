using System;

namespace Trail365.ViewModels
{
    public class GpxFileViewModel
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string FileName { get; set; }
        public string AbsoluteUrl { get; set; }
    }
}
