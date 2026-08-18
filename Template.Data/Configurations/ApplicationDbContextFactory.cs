using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Template.Data.Configurations
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Get the directory where ApplicationDbContextFactory.cs actually resides
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Locate the API directory (moves up out of bin/Debug and into Template.Api)
            string apiProjectPath = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\Template.Api"));

            // If appsettings.json isn't found at that path, fall back to current directory
            if (!File.Exists(Path.Combine(apiProjectPath, "appsettings.json")))
            {
                apiProjectPath = Directory.GetCurrentDirectory();
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Get connection string from appsettings.json, or fallback to local string if null
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? @"Server=localhost\SQLEXPRESS;Database=CaseManagementDb;Trusted_Connection=True;TrustServerCertificate=True;";

            builder.UseSqlServer(connectionString);
            
            return new ApplicationDbContext(builder.Options);
        }
    }
}