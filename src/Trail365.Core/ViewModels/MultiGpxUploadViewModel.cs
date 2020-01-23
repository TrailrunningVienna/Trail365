using System.Collections.Generic;

namespace Trail365.ViewModels
{
    public class MultiGpxUploadViewModel
    {
        public int UnreadMessages { get; set; }

        public LoginViewModel Login { get; set; } = new LoginViewModel();

        public List<GpxFileViewModel> FileInfos { get; set; } = new List<GpxFileViewModel>();
    }
}
