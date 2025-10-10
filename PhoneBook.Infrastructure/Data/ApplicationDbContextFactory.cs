using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PhoneBook.Infrastructure.Data
{
    /// <summary>
    /// Factory for creating DbContext at design time (for migrations)
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../PhoneBook.Web");
            
            // If running from Infrastructure project directory
            if (!Directory.Exists(basePath))
            {
                basePath = Path.Combine(Directory.GetCurrentDirectory(), "PhoneBook.Web");
            }
            
            // If still not found, try parent directory
            if (!Directory.Exists(basePath))
            {
                var parentPath = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName;
                if (parentPath != null)
                {
                    basePath = Path.Combine(parentPath, "PhoneBook.Web");
                }
            }

            Console.WriteLine($"Looking for appsettings.json in: {basePath}");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string 'DefaultConnection' not found in appsettings.json at {basePath}");
            }

            Console.WriteLine($"Using connection string: {connectionString}");

            // Build DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString, 
                b => b.MigrationsAssembly("PhoneBook.Infrastructure"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}