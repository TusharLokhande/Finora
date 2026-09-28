using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Common.Services;
using Microsoft.Extensions.Configuration;

namespace MoneyManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IValidationService, ValidationService>();

        services.Configure<Common.ImportOptions>(configuration.GetSection("Import"));
        services.Configure<Common.JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<Common.GoogleAuthOptions>(configuration.GetSection("GoogleAuth"));
        services.Configure<Common.AuthOptions>(configuration.GetSection("Auth"));

        var mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(typeof(DependencyInjection).Assembly);
        services.AddSingleton(mapsterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
