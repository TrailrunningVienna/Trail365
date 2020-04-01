using System;
using System.ComponentModel.DataAnnotations;

namespace Trail365.Entities
{
    public class EventInvolvement
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EventID { get; set; }

        public Event Event { get; set; }

        [Required]
        public Guid UserID { get; set; }

        public User User { get; set; }

        [Required]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public Guid? InvitedByUserID { get; set; }

        public string CreatedByUser { get; set; } = System.Threading.Thread.CurrentPrincipal?.Identity.Name;

        public DateTime? ModifiedUtc { get; set; }

        public string ModifiedByUser { get; set; }

        [Required]
        public InvolvedLevel Level { get; set; } = InvolvedLevel.Invited;
    }
}
