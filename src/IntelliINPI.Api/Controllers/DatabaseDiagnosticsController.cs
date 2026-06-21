using IntelliINPI.Infrastructure.Persistence;
using IntelliINPI.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/diagnostics/database")]
public sealed class DatabaseDiagnosticsController(
    ApplicationDbContext dbContext,
    DatabaseConnectionSettings settings,
    IWebHostEnvironment environment,
    IConfiguration configuration,
    ILogger<DatabaseDiagnosticsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Check(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment() && !configuration.GetValue("ENABLE_DIAGNOSTICS", false))
        {
            return NotFound();
        }

        try
        {
            await dbContext.Database.OpenConnectionAsync(cancellationToken);
            await dbContext.Database.SqlQueryRaw<int>("SELECT 1 AS \"Value\"").SingleAsync(cancellationToken);

            return Ok(new
            {
                status = "ok",
                provider = settings.Provider,
                databaseReachable = true
            });
        }
        catch (Exception exception) when (DatabaseFailureClassifier.IsUnavailable(exception))
        {
            logger.LogWarning(exception, "Database diagnostic failed. Provider={Provider} Host={Host} Database={Database}",
                settings.Provider, settings.MaskedHost, settings.Database);

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "error",
                provider = settings.Provider,
                databaseReachable = false,
                error = Summarize(exception)
            });
        }
        finally
        {
            await dbContext.Database.CloseConnectionAsync();
        }
    }

    private static string Summarize(Exception exception)
    {
        if (exception is PostgresException postgresException)
        {
            return postgresException.SqlState switch
            {
                PostgresErrorCodes.InvalidPassword => "Database authentication failed",
                PostgresErrorCodes.InvalidCatalogName => "Database does not exist",
                PostgresErrorCodes.UndefinedTable => "Database schema is not initialized",
                _ => "Database rejected the connection or query"
            };
        }

        var text = exception.ToString();
        if (text.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase)
            || text.Contains("No such host", StringComparison.OrdinalIgnoreCase))
        {
            return "Database host could not be resolved";
        }

        if (text.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            return "Database connection timed out";
        }

        return "Could not connect to database";
    }
}
