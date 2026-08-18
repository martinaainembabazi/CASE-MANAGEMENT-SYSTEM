using System.Reflection;
using Template.Core.Repository.Accounts;
using Template.Core.Repository.ApplicationPermission;
using Template.Core.Repository.ApplicationPermissions;
using Template.Core.Repository.Auditable;
using Template.Core.Repository.AuditLogs;
using Template.Core.Repository.Roles;
using Template.Core.Services.AdAuthentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Template.Core;

public static class CoreServicesRegistration
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
	{ 
        // Register HttpContextAccessor if not already registered
		services.AddHttpContextAccessor();
		services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IApplicationPermissionRepository, ApplicationPermissionRepository>();
        services.AddScoped<IAuthorizationHandler, ApplicationPermissionHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, ApplicationPermissionPolicyProvider>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }

	// Add the new extension method in the same static class
	public static IServiceCollection AddDbContextWithAuditing<TContext>(
		this IServiceCollection services,
		Action<DbContextOptionsBuilder> optionsAction = null)
		where TContext : DbContext
	{
		return services.AddDbContext<TContext>((sp, options) =>
		{
			optionsAction?.Invoke(options);

			// Add the interceptor
			var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
			options.AddInterceptors(new AuditSaveChangesInterceptor(httpContextAccessor));
		});
	}
}

