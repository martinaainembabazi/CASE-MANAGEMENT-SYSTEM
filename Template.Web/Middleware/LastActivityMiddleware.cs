using System.Security.Claims;
using Template.Data.Configurations;
using Template.Data.Entities;

public class LastActivityMiddleware
{
    private readonly RequestDelegate _next;

    public LastActivityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await dbContext.Users.FindAsync(userId);
                if (user != null)
                {
                    await UpdateLastActivityIfDueAsync(user, dbContext);
                }
            }
        }

        await _next(context);
    }

    private static async Task UpdateLastActivityIfDueAsync(ApplicationUser user, ApplicationDbContext dbContext)
    {
        var now = DateTime.Now;
        if (user.LastActivity < now.AddMinutes(-10))
        {
            user.LastActivity = now;
            await dbContext.SaveChangesAsync();
        }
    }
}
