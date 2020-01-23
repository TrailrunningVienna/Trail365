using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trail365.Entities
{
    public class TaskLogItem
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        [Display(Name = "Timestamp")]
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        [Display(Name = "Message")]
        public string LogMessage { get; set; }

        public string Level { get; set; }

        public string Category { get; set; }

        [NotMapped]
        public string LogMessagePreview
        {
            get
            {
                if (string.IsNullOrEmpty(this.LogMessage)) return string.Empty;
                if (this.LogMessage.Length < 250) return this.LogMessage;
                return this.LogMessage.Substring(0, 249) + " ....";
            }
        }
    }
}
