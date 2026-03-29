using Hospital.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Hospital.Web.Infrastructure.Claims;

/// <summary>
/// Adds a "FullName" claim so the layout can display the user's real name
/// instead of their email/username.
/// </summary>
public sealed class ApplicationUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser>  userManager,
        RoleManager<IdentityRole>     roleManager,
        IOptions<IdentityOptions>     optionsAccessor)
        : base(userManager, roleManager, optionsAccessor) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Full display name — falls back to email if Name not set
        var fullName = !string.IsNullOrWhiteSpace(user.Name)
            ? user.Name
            : (user.Email ?? user.UserName ?? "User");

        identity.AddClaim(new Claim("FullName", fullName));

        // Add specialist if doctor
        if (user.IsDoctor && !string.IsNullOrWhiteSpace(user.Specialist))
            identity.AddClaim(new Claim("Specialist", user.Specialist));

        return identity;
    }
}
