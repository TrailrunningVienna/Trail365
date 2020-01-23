using System;

namespace Trail365.ViewModels
{
    public class CreateTrailViewModel
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string FileUrl { get; set; }
        public Guid? GpxID { get; set; }
        public LoginViewModel Login { get; set; } = new LoginViewModel();
        public string Name { get; set; }
    }
}
