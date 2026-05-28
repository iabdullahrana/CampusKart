using System.Security.Claims;
using CampusKart.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CampusKart.Components.Account
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public CustomUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            
            // Inject custom user properties from database directly into cookie claims
            identity.AddClaim(new Claim("picture", user.ProfilePictureUrl ?? ""));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName ?? user.UserName ?? ""));
            
            return identity;
        }
    }
}
