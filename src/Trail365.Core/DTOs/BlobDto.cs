using System;
using Trail365.Entities;

namespace Trail365.DTOs
{

    public class UserMessage
    {
        public int MessageID { get; set; }
        public int SYSUserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MessageText { get; set; }
        public DateTime? LogDate { get; set; }
    }

    public class BlobDto
    {
        public static BlobDto FromBlob(Blob blob)
        {
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            var dto = new BlobDto
            {
                ID = blob.ID
            };

            dto.Url = blob.Url;
            dto.ImageWidth = blob.ImageWidth;
            dto.ImageHeight = blob.ImageHeight;
            dto.SubFolder = blob.FolderName;
            dto.OriginalFileName = blob.OriginalFileName;
            dto.MimeType = blob.MimeType;
            return dto;
        }

        public BlobDto()
        {
        }

        public Guid ID { get; set; } = Guid.NewGuid();

        public string SubFolder { get; set; }

        public byte[] Data { get; set; }

        public string Url { get; set; }

        public int? ImageHeight { get; set; }

        public int? ImageWidth { get; set; }

        public string MimeType { get; set; }

        public string OriginalFileName { get; set; }
    }
}
