using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Accounts.Interfaces;
using MoneyManagement.Application.Features.Auth.Interfaces;
using MoneyManagement.Application.Features.Budgets.Interfaces;
using MoneyManagement.Application.Features.Categories.Interfaces;
using MoneyManagement.Application.Features.Transactions.Interfaces;
using MoneyManagement.Application.Interfaces.Repository;
using MoneyManagement.Application.Interfaces.UnitOfWork;
using MoneyManagement.Infrastructure.Auth;
using MoneyManagement.Infrastructure.Export;
using MoneyManagement.Infrastructure.FileStorage;
using MoneyManagement.Infrastructure.Monitoring.HealthChecks;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;
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

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IExcelExportWriter, ExcelExportWriter>();

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

    public static async Task SeedDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        const string adminEmail = "tlokhande00@gmail.com";
        var admin = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (admin is null)
        {
            dbContext.Users.Add(new User
            {
                Name = "Tushar Lokhande",
                Email = adminEmail,
                Role = UserRole.Admin,
                Status = UserStatus.Approved,
            });
        }
        else if (admin.Role != UserRole.Admin)
        {
            admin.Role = UserRole.Admin;
            admin.UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();
    }
}
