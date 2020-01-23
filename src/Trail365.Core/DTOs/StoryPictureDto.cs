using System;

namespace Trail365.DTOs
{
    public class StoryPictureDto
    {
        //or encapsulated as Image ? => server side
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid StoryID { get; set; }

        public string ImageType { get; set; }

        public byte[] Data { get; set; }

        public string Url { get; set; }

        public int? Height { get; set; }

        public int? Width { get; set; }
    }
}
