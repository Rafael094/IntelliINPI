using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using IntelliINPI.Infrastructure.Services;

namespace IntelliINPI.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(Path.Combine("src", "IntelliINPI.Api", "appsettings.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();
        var databaseSettings = DatabaseConnectionSettings.Load(configuration);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(databaseSettings.ConnectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
