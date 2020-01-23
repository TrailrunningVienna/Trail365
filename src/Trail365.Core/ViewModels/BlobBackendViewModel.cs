using System;
using System.Collections.Generic;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class BlobBackendViewModel
    {
        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }

        public string Url { get; set; }
        public Guid ID { get; set; } = Guid.NewGuid();

        public static BlobBackendViewModel CreateFromBlob(Blob item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            return new BlobBackendViewModel
            {
                ID = item.ID,
                Url = item.Url,
                Created = item.CreatedUtc.ToLocalTime(),
                Modified = item.ModifiedUtc.ToLocalTime(),
            };
        }

        public List<Tuple<string, string>> References { get; set; } = new List<Tuple<string, string>>();

        public Blob ApplyChangesTo(Blob item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            item.ModifiedUtc = DateTime.UtcNow;
            item.Url = this.Url;
            return item;
        }
    }
}
