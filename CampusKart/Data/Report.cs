using System;
using System.ComponentModel.DataAnnotations;

namespace CampusKart.Data
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        public int ListingId { get; set; }

        [StringLength(100)]
        public string ListingTitle { get; set; } = "";

        [StringLength(4000)]
        public string ListingImageUrl { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string Reason { get; set; } = "";

        [StringLength(2000)]
        public string Description { get; set; } = "";

        [StringLength(100)]
        public string ReporterName { get; set; } = "";

        [StringLength(100)]
        public string ReporterEmail { get; set; } = "";

        [Required]
        public DateTime ReportedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending | Resolved

        public DateTime? ResolvedDate { get; set; }
    }
}
