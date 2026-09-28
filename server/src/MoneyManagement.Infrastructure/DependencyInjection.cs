using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Interfaces.Repository;
using MoneyManagement.Application.Interfaces.UnitOfWork;
using MoneyManagement.Infrastructure.Auth;
using MoneyManagement.Infrastructure.FileStorage;
using MoneyManagement.Infrastructure.Monitoring.HealthChecks;
using MoneyManagement.Infrastructure.Persistence;
using MoneyManagement.Infrastructure.Persistence.Repository;

namespace MoneyManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpClient<IGoogleOAuthClient, GoogleOAuthClient>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
        services.AddSingleton(new BlobServiceClient(configuration.GetConnectionString("AzureStorage")));
        services.AddScoped<IFileStorageService, AzureBlobFileStorageService>();

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
