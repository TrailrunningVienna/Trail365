using System;

namespace Trail365.ViewModels
{
    public class StoryBlockFileViewModel
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string FileName { get; set; }
        public string AbsoluteUrl { get; set; }
        public string Extension { get; set; }

        public string GetFileTypeCssClass()
        {
            string ext = $"{this.Extension}".ToLowerInvariant().TrimStart('.').Trim();
            if (ext == "txt")
            {
                return "fa fa-file-text";
            }
            if ((ext == "jpg") || (ext == "png"))
            {
                return "fa fa-file-photo-o";
            }
            if (ext == "md")
            {
                return "fa fa-file-code-o";
            }
            return "fa fa-file";
        }
    }
}
