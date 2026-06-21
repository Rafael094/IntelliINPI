using FluentValidation;
using IntelliINPI.Application.Common.Exceptions;

namespace IntelliINPI.Api;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { errors = exception.Errors.Select(x => x.ErrorMessage) });
        }
        catch (UnauthorizedAppException exception)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message });
        }
        catch (NotFoundException exception)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message });
        }
        catch (ConfigurationAppException exception)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { message = exception.Message });
        }
        catch (Exception exception) when (DatabaseFailureClassifier.IsUnavailable(exception))
        {
            logger.LogError(exception, "Database operation failed for {Method} {Path}", context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Database unavailable",
                details = "Could not connect to database"
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled error for {Method} {Path}", context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "Internal server error" });
        }
    }
}
