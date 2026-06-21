using IntelliINPI.Api;
using IntelliINPI.Application.DependencyInjection;
using IntelliINPI.Infrastructure.DependencyInjection;
using IntelliINPI.Infrastructure.Persistence;
using IntelliINPI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });
});

var app = builder.Build();

var databaseSettings = app.Services.GetRequiredService<DatabaseConnectionSettings>();
app.Logger.LogInformation(
    "Database config loaded. Provider={Provider}; Host={Host}; Database={Database}; SSL={SslMode}",
    databaseSettings.Provider,
    databaseSettings.MaskedHost,
    databaseSettings.Database,
    databaseSettings.SslMode);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var applyMigrations = builder.Configuration.GetValue("APPLY_MIGRATIONS_ON_STARTUP", false);
var seedAdmin = app.Environment.IsDevelopment() || builder.Configuration.GetValue("SEED_ADMIN_ON_STARTUP", false);
if (applyMigrations || seedAdmin)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (applyMigrations)
        {
            await dbContext.Database.MigrateAsync();
        }

        if (seedAdmin)
        {
            await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
        }
    }
    catch (Exception exception)
    {
        app.Logger.LogError(exception,
            "Database startup initialization failed. The API will remain available for health and diagnostics.");
    }
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
