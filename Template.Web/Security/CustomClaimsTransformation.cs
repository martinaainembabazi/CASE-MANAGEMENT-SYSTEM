using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Template.Data.Configurations;

namespace Template.Web.Security;

public class CustomClaimsTransformation : IClaimsTransformation
{
    private readonly ApplicationDbContext _context;

    public CustomClaimsTransformation(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null || !identity.IsAuthenticated)
        {
            return principal;
        }

        // Prevent adding duplicate role claims if already present
        if (identity.HasClaim(c => c.Type == ClaimTypes.Role))
        {
            return principal;
        }

        var userIdString = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdString, out int userId) || Guid.TryParse(userIdString, out Guid userGuid))
        {
            // Fetch the role name from your custom Role entity
            var userRoleName = await _context.Users
                .Where(u => u.Id.ToString() == userIdString)
                .Select(u => u.Role.Name)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(userRoleName))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, userRoleName));
            }
        }

        return principal;
    }
}