using System;
using System.ComponentModel.DataAnnotations;

namespace Trail365.Entities
{
    public class FederatedIdentity
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid UserID { get; set; }
        public User User { get; set; }

        /// <summary>
        /// name derived from ClaimTypes.NameIdentifier => value comes from there
        /// Unique only TOGETHER with AuthenticationType
        /// </summary>
        public string Identifier { get; set; }

        public string AuthenticationType { get; set; }

        public string Email { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
