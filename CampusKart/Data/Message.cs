using System;
using System.ComponentModel.DataAnnotations;

namespace CampusKart.Data
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ListingId { get; set; }

        [Required]
        [StringLength(100)]
        public string SenderEmail { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string ReceiverEmail { get; set; } = "";

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = "";

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
