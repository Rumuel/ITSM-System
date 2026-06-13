using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace Infrastructure.Data
{
    public class ItsmDbContextFactory : IDesignTimeDbContextFactory<ItsmDbContext>
    {
        public ItsmDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ItsmDbContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            return new ItsmDbContext(optionsBuilder.Options);
        }

        private static string GetConnectionString()
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            var appSettingsPaths = new[]
            {
                Path.Combine(currentDirectory, "..", "api", "appsettings.json"),
                Path.Combine(currentDirectory, "backend", "ITSM", "api", "appsettings.json"),
                Path.Combine(currentDirectory, "appsettings.json")
            };

            foreach (var path in appSettingsPaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (!File.Exists(fullPath))
                {
                    continue;
                }

                using var json = JsonDocument.Parse(File.ReadAllText(fullPath));
                if (json.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
                    && connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
                {
                    connectionString = defaultConnection.GetString();
                    if (!string.IsNullOrWhiteSpace(connectionString))
                    {
                        return connectionString;
                    }
                }
            }

            throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        }
    }
}
