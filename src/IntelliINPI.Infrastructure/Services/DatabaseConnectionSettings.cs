using Microsoft.Extensions.Configuration;
using Npgsql;

namespace IntelliINPI.Infrastructure.Services;

public sealed record DatabaseConnectionSettings(
    string ConnectionString,
    string Provider,
    string Host,
    string Database,
    string SslMode)
{
    public string MaskedHost => MaskHost(Host);

    public static DatabaseConnectionSettings Load(IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL") ?? configuration["DATABASE_URL"];
            connectionString = ConvertDatabaseUrl(databaseUrl);
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection not configured. Set ConnectionStrings__DefaultConnection or DATABASE_URL.");
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        if ((builder.Host ?? string.Empty).Contains("supabase", StringComparison.OrdinalIgnoreCase))
        {
            builder.SslMode = Npgsql.SslMode.Require;
        }

        builder.Timeout = builder.Timeout <= 0 ? 30 : builder.Timeout;
        builder.CommandTimeout = builder.CommandTimeout <= 0 ? 60 : builder.CommandTimeout;
        builder.Pooling = true;
        builder.MinPoolSize = Math.Max(0, builder.MinPoolSize);
        builder.MaxPoolSize = Math.Clamp(builder.MaxPoolSize, 1, 20);

        return new DatabaseConnectionSettings(
            builder.ConnectionString,
            "Npgsql",
            builder.Host ?? string.Empty,
            builder.Database ?? string.Empty,
            builder.SslMode.ToString());
    }

    private static string? ConvertDatabaseUrl(string? databaseUrl)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            return databaseUrl;
        }

        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = uri.AbsolutePath.Trim('/'),
            Username = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? string.Empty),
            Password = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? string.Empty),
            SslMode = Npgsql.SslMode.Require,
            Timeout = 30,
            CommandTimeout = 60,
            Pooling = true,
            MinPoolSize = 0,
            MaxPoolSize = 20
        };

        return builder.ConnectionString;
    }

    private static string MaskHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host) || host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return host;
        }

        var parts = host.Split('.');
        if (parts.Length < 3)
        {
            return host.Length <= 6 ? "***" : $"{host[..3]}***{host[^2..]}";
        }

        var first = parts[0];
        var maskedFirst = first.Length <= 4 ? "***" : $"{first[..2]}***{first[^2..]}";
        return string.Join('.', new[] { maskedFirst }.Concat(parts.Skip(1)));
    }
}
