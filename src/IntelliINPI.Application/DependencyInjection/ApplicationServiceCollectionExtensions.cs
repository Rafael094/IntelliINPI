using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Behaviors;
using IntelliINPI.Application.Imports;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace IntelliINPI.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceCollectionExtensions).Assembly;
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IInpiOpenDataTrademarkImporter, InpiOpenDataTrademarkImporter>();
        services.AddScoped<IInpiRpiTrademarkImporter, InpiRpiTrademarkImporter>();
        services.AddScoped<ITrademarkImporter>(provider => provider.GetRequiredService<IInpiOpenDataTrademarkImporter>());
        return services;
    }
}
