using Microsoft.AspNetCore.Identity;

namespace CampusKart.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    }
}
