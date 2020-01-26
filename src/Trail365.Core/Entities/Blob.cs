using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Trail365.DTOs;

namespace Trail365.Entities
{
    /// <summary>
    /// Blob
    /// </summary>
    public class Blob
    {
        public static Blob FromDto(BlobDto blobDto)
        {
            if (blobDto == null) throw new ArgumentNullException(nameof(blobDto));
            Blob i = new Blob
            {
                ID = blobDto.ID,
                FolderName = blobDto.SubFolder,
                Url = blobDto.Url,
                MimeType = blobDto.MimeType,
                OriginalFileName = blobDto.OriginalFileName,
            };

            i.ImageWidth = blobDto.ImageWidth;
            i.ImageHeight = blobDto.ImageHeight;
            i.StorageSize = blobDto.Data?.Length;
            return i;
        }

        public Blob()
        { }

        /// <summary>
        /// used as Blob-Guid
        /// </summary>
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public string FolderName { get; set; }

        public string MimeType { get; set; }

        /// <summary>
        /// With Extension!
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// specially for images
        /// </summary>
        public int? ImageHeight { get; set; }

        /// <summary>
        /// specially for images
        /// </summary>
        public int? ImageWidth { get; set; }

        /// <summary>
        /// Default is Size.Empty 
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Size GetImageSizeOrDefault()
        {
            if ((!this.ImageHeight.HasValue) || (!this.ImageWidth.HasValue))
            {
                return System.Drawing.Size.Empty;
            }
            return new System.Drawing.Size(this.ImageWidth.Value, this.ImageHeight.Value);
        }
        public int? StorageSize { get; set; }

        public void AssignImageSize(System.Drawing.Size value)
        {
            if (value.IsEmpty)
            {
                this.ImageHeight = null;
                this.ImageWidth = null;
            }
            else
            {
                this.ImageHeight = value.Height;
                this.ImageWidth = value.Width;
            }
        }

        /// <summary>
        /// owned by this trail, but not speciefied in which property/purpose
        /// </summary>
        public Guid? OwningTrailID { get; set; }

        [Required]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public string CreatedByUser { get; set; } = System.Threading.Thread.CurrentPrincipal?.Identity.Name;

        public DateTime? ModifiedUtc { get; set; }

        public string ModifiedByUser { get; set; }

        /// <summary>
        /// Storage/Download Url
        /// </summary>
        public string Url { get; set; }

        public List<StoryBlock> StoryBlocks { get; set; } = new List<StoryBlock>();

        public List<Story> StoryCovers { get; set; } = new List<Story>();

        public string ContentHash { get; set; }

        public string GetShortContentHash()
        {
            return this.ContentHash.Substring(0, 7);
        }

        public string GetHumanizedUrl()
        {
            if (string.IsNullOrEmpty(this.Url)) return "<empty>";
            int len = this.Url.Length;
            int lastX = 20;
            if (len < lastX)
            {
                return this.Url;
            }
            return "..." + this.Url.Substring(len - lastX);
        }

        public string GetHumanizedStorageSize()
        {
            if (!this.StorageSize.HasValue) return "<empty>";
            if (this.StorageSize.Value >= (1024 * 1024))
            {
                return $"{Convert.ToInt32(this.StorageSize.Value / (1024 * 1024)).ToString()} MBytes";
            }

            if (this.StorageSize.Value >= (1024))
            {
                return $"{Convert.ToInt32(this.StorageSize.Value / (1024)).ToString()} KBytes";
            }

            return $"{this.StorageSize.Value} Bytes";
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(this.FolderName)) throw new ValidationException($"{nameof(this.FolderName)} cannot be empty");
            if (string.IsNullOrEmpty(this.MimeType)) throw new ValidationException($"{nameof(this.MimeType)} cannot be empty");
            if (string.IsNullOrEmpty(this.ContentHash)) throw new ValidationException("ContentHash should not be empty");
            if (this.StorageSize.HasValue == false) throw new ValidationException("StorageSize should not be empty");
        }
    }
}
