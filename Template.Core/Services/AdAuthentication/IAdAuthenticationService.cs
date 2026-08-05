using Template.Core.Models.Account;

namespace Template.Core.Services.AdAuthentication
{
    public interface IAdAuthenticationService
    {
        bool ValidateCredentials(string username, string password);
        bool ValidateUserCredentials(string username, string password);

        AdUserResult IsExistsOnAd(ApplicationUserViewModel model);

    }
}
