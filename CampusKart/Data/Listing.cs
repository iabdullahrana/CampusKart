using System;
using System.ComponentModel.DataAnnotations;

namespace CampusKart.Data
{
    public class Listing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Condition { get; set; } = "";

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = "";

        [Required]
        public int Price { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string SellerName { get; set; } = "";

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string SellerEmail { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = "";

        [Required]
        public DateTime DatePosted { get; set; } = DateTime.UtcNow;
    }
}
