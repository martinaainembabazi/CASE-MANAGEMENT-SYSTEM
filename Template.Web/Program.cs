using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog.Web;
using SmartBreadcrumbs.Extensions;
using System.Reflection;
using Template.Core.Mappings;
using Template.Core.Repository;
using Template.Core.Repository.Cases;
using Template.Core.Services.AdAuthentication;
using Template.Data;
using Template.Data.Configurations;
using Template.Data.Entities;
var builder = WebApplication.CreateBuilder(args);

// Register Repositories
builder.Services.AddScoped<ICaseRepository, CaseRepository>();

builder.Logging.ClearProviders();
builder.Host.UseNLog();
builder.Services.AddLogging();


builder.Configuration.AddJsonFile("appsettings.json");

// Add services to the container.
builder.Services.AddControllersWithViews();

//Configure Blazor
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => options.DetailedErrors = true);
builder.Services.AddBlazorBootstrap();

builder.Services.AddAuthentication("CookieAuth")
            .AddCookie("CookieAuth", options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                //options.AccessDeniedPath = "/Account/Login";
            });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(365);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//Add configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.SlidingExpiration = true;
});

//builder.Services.AddAuthorizationBuilder();
builder.Services.AddAuthorization();

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(CasesAutoMapperProfile).Assembly)); // pass the assembly using configuration action

builder.Services.AddScoped<ILawFirmRepository, LawFirmRepository>();

// Register the LDAP authentication service with the interface

builder.Services.AddSingleton<IAdAuthenticationService>(provider =>
    new AdAuthenticationService(
        "SVRHQSDC001", // server name/ip address 
        "DC=BCNET,DC=BOU,DC=OR,DC=UG", // LDAP container
        provider.GetRequiredService<ILogger<AdAuthenticationService>>()
    ));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

//Template.Data Service settings
DataServicesRegistration.AddDataServices(builder.Services, builder.Configuration);

//Template.Core Service settings
CoreServicesRegistration.AddCoreServices(builder.Services);

builder.Services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options =>
{
	options.TagName = "nav";
	options.TagClasses = "";
	options.OlClasses = "breadcrumb";
	options.LiClasses = "breadcrumb-item";
	options.ActiveLiClasses = "breadcrumb-item active";
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));

    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Add response caching middleware
app.UseResponseCaching();

// Configure static files with caching

//app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static resources for 7 days
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
    }
});

// Apply no-cache only to dynamic content
//app.Use(async (context, next) =>
//{
//    // Don't modify caching for static files
//    if (!context.Request.Path.StartsWithSegments("/css") &&
//        !context.Request.Path.StartsWithSegments("/js") &&
//        !context.Request.Path.StartsWithSegments("/lib") &&
//        !context.Request.Path.StartsWithSegments("/images") &&
//        !context.Request.Path.StartsWithSegments("/fonts"))
//    {
//        context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
//        context.Response.Headers["Pragma"] = "no-cache";
//        context.Response.Headers["Expires"] = "0";
//    }
//    await next();
//});

app.UseHttpsRedirection();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
//app.UseMiddleware<LastActivityMiddleware>();
app.MapIdentityApi<ApplicationUser>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}/{param1?}");

app.MapBlazorHub();

// Seed roles and default users on startup
await DbInitializer.SeedAsync(app.Services);

app.Run();
