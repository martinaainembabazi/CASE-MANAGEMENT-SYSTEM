using System.DirectoryServices.AccountManagement;
using Template.Core.Models.Account;
using Template.Data.Entities;
using Microsoft.Extensions.Logging;

namespace Template.Core.Services.AdAuthentication
{
    public class AdAuthenticationService(string _ldapServer
        , string _ldapContainer
        , ILogger<AdAuthenticationService> _logger
        ) : IAdAuthenticationService
    {
        public bool ValidateCredentials(string username, string password)
        {
            try
            {
                //using (var context = new PrincipalContext(ContextType.Domain, _ldapServer, _ldapContainer))
                //{
                //    return context.ValidateCredentials(username, password);
                //}
                return true;
            }
            catch (PrincipalServerDownException ex)
            {
                // Handle connection issues with LDAP server
                // Log the exception
                //_logger.LogError(ex, "LDAP server is down or unreachable. Server: {LdapServer}", _ldapServer);
                return false;
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                // Log the exception
                //_logger.LogError(ex, "An unexpected error occurred during LDAP authentication. Username: {Username}", username);
                return false;
            }
        }

        public bool ValidateUserCredentials(string username, string password)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _ldapServer, _ldapContainer))
                {
                    // Attempt to validate the credentials
                    bool isValid = context.ValidateCredentials(username, password);

                    if (isValid)
                    {
                        return true; // Login successful
                    }
                    else
                    {
                        // Check if the account is now locked
                        var user = UserPrincipal.FindByIdentity(context, username);

                        if (user != null && user.IsAccountLockedOut())
                        {
                            _logger.LogError($"User account {username} has been locked due to three incorrect login attempts.");
                        }

                        return false; // Credentials are invalid
                    }
                }
            }
            catch (PrincipalServerDownException ex)
            {
                // Log the exception (uncomment if logging is in place)
                // _logger.LogError(ex, "LDAP server is down or unreachable. Server: {LdapServer}", _ldapServer);
                return false;
            }
            catch (Exception ex)
            {
                // Log other unexpected exceptions
                // _logger.LogError(ex, "An unexpected error occurred during LDAP authentication. Username: {Username}", username);
                return false;
            }
        }

        public AdUserResult IsExistsOnAd(ApplicationUserViewModel model)
        {
            try
            {
                // Create a PrincipalContext for the domain
                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    // Search for the user by SAM account name
                    var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, model.UserName);

                    // If the user is found, return true; otherwise, return false
                    if (user != null)
                    {
                        ApplicationUser appUser = new ApplicationUser()
                        {
                            UserName = model.UserName,
                            EndDate = model.EndDate,
                            FirstName = user.GivenName,
                            MiddleName = user.MiddleName,
                            LastName = user.Surname,
                            Email = user.EmailAddress,
                            Title = model.Title,
                        };

                        return new AdUserResult
                        {
                            User = user,
                            AppUser = appUser,
                            IsSuccess = true,
                            ResultType = AdResultTypes.Success
                        };
                    }
                    else
                    {
                        // User not found
                        //return null;
                        return new AdUserResult
                        {
                            IsSuccess = false,
                            ErrorMessage = $"User '{model.UserName}' not found in Active Directory",
                            ResultType = AdResultTypes.UserNotFound
                        };
                    }
                }
            }
            catch (PrincipalServerDownException ex)
            {
                //Console.WriteLine("Unable to connect to the domain controller.");
                //return null;
                return new AdUserResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Unable to connect to the Active Directory server",
                    Exception = ex,
                    ResultType = AdResultTypes.ServerUnavailable
                };
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"An error occurred: {ex.Message}");
                //return null;
                return new AdUserResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while checking Active Directory: {ex.Message}",
                    Exception = ex,
                    ResultType = AdResultTypes.OtherError
                };
            }
        }

    }
}
