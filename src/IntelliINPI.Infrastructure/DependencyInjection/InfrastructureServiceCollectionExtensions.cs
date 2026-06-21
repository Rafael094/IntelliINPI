using System.Net;
using System.Text;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Infrastructure.Persistence;
using IntelliINPI.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace IntelliINPI.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseSettings = DatabaseConnectionSettings.Load(configuration);
        services.AddSingleton(databaseSettings);
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(databaseSettings.ConnectionString));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<INitAiService, NitAiServiceStub>();
        services.AddScoped<INitInpiIntegrationService, NitInpiIntegrationServiceStub>();
        services.AddScoped<INitSignatureService, NitSignatureServiceStub>();
        services.AddScoped<INitNotificationService, NitNotificationServiceStub>();
        services.AddScoped<INitDocumentStorage, NitDocumentStorage>();
        services.AddScoped<IInpiSearchService, InpiSearchService>();
        services.AddHttpContextAccessor();
        services.Configure<InpiOpenDataOptions>(configuration.GetSection("InpiOpenData"));
        services.Configure<InpiRpiOptions>(configuration.GetSection("InpiRpi"));
        services.Configure<InpiRpiHistoryOptions>(configuration.GetSection("InpiRpiHistory"));
        services.Configure<ExternalSearchOptions>(configuration.GetSection("ExternalSearch"));
        services.AddSingleton<IInpiRpiHistorySettings, InpiRpiHistorySettings>();
        services.AddHttpClient<IInpiOpenDataClient, InpiOpenDataClient>();
        services.AddHttpClient<IInpiRpiClient, InpiRpiClient>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                AllowAutoRedirect = true
            });

        var secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret não configurado.");
        if (string.Equals(configuration["ExternalSearch:Provider"], "Exa", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(configuration["ExternalSearch:ApiKey"]))
        {
            services.AddHttpClient<IExternalBrandSearchClient, ExaBrandSearchClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.exa.ai/");
                client.Timeout = TimeSpan.FromSeconds(configuration.GetValue("ExternalSearch:TimeoutSeconds", 20));
            });
        }
        else
        {
            services.AddScoped<IExternalBrandSearchClient, DisabledExternalBrandSearchClient>();
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "IntelliINPI",
                    ValidAudience = configuration["Jwt:Audience"] ?? "IntelliINPI",
                    IssuerSigningKey = key
                };
            });

        return services;
    }
}
