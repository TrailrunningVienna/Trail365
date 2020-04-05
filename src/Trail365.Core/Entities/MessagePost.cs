using System;
using System.ComponentModel.DataAnnotations;

namespace Trail365.Entities
{
    public class MessagePost
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? ParentMessagePostID { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public string CreatedByUser { get; set; } = System.Threading.Thread.CurrentPrincipal?.Identity.Name;

        [Required]
        public Guid CreatedByUserID { get; set; }

        public DateTime? ModifiedUtc { get; set; }

        public string ModifiedByUser { get; set; }

    }
}
