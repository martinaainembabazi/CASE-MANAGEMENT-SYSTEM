using Template.Core.Repository.Accounts;
using Template.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Template.Core.Services.AdAuthentication;
public class AuthService(
     IAdAuthenticationService _adAuthService
    , IAccountRepository _accountRepo
    , ILogger<AuthService> _logger
    , UserManager<ApplicationUser> _userManager
    , IHttpContextAccessor _httpContextAccessor
    , SignInManager<ApplicationUser> _signInManager
    , IHostEnvironment _environment) : IAuthService
{
    public async Task SignInApplicationUser(ApplicationUser user, bool isPersistent = false)
    {
        user.IsLoggedIn = true;
        user.LastActivity = DateTime.Now;
        _httpContextAccessor.HttpContext.Session.SetString("userName", user.UserName);

        await _userManager.UpdateAsync(user);
        await _signInManager.SignInAsync(user, isPersistent);
    }
    public async Task SignOutApplicationUser()
    {
        var userName = _httpContextAccessor.HttpContext.Session.GetString("userName");
        var user = await _accountRepo.FindByName(userName);

        if (user != null)
        {
            user.IsLoggedIn = false;
            await _userManager.UpdateAsync(user);
        }

        await _signInManager.SignOutAsync();
        
        _httpContextAccessor.HttpContext?.Session.Clear();
    }

    public async Task<(bool Success, string Status, ApplicationUser User)> ValidateApplicationUser(string username, string password)
    {
        // Find user
        var user = await _accountRepo.FindByName(username);
        if (user == null)
        {
            _logger.LogError($"Failed login attempt by {username}. User does not exist.");
            return (false, "Wrong username or password.", null);
        }

        // Development uses the password stored by ASP.NET Identity so the seeded
        // local accounts work without access to the organization's LDAP server.
        // Non-development environments continue to require Active Directory.
        var credentialsAreValid = _environment.IsDevelopment()
            ? await _userManager.CheckPasswordAsync(user, password)
            : _adAuthService.ValidateCredentials(username, password);

        if (!credentialsAreValid)
        {
            _logger.LogWarning(
                "Failed {AuthenticationType} authentication for username {Username}.",
                _environment.IsDevelopment() ? "local" : "Active Directory",
                username);
            return (false, "Wrong username or password.", null);
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _logger.LogError($"Failed login attempt by {username}. Account inactive.");
            return (false, "Account inactive.", null);
        }

        // Check if user account is disabled
        if (user.DisableDate < DateTime.Now)
        {
            _logger.LogError($"Failed login attempt by {username}. Account disabled.");
            return (false, "Account disabled.", null);
        }

        //Check if account has expired
        if (user.EndDate < DateTime.Now)
        {
            _logger.LogError($"Failed login by {username}, account was disabled because it reached its end date.");
            return (false, "Account expired.", null);
        }

        //Check it user is already logged in
        if (user.IsLoggedIn == true)
        {
            if (user.LastActivity < DateTime.UtcNow.AddMinutes(-15))
            {
                return (true, "success", user);
            }

            _logger.LogError($"Attempted login by {username} for already logged in user.");
            return (false, "You are already logged in using another browser or device.", null);
        }

        return (true, "success", user);

    }
}
