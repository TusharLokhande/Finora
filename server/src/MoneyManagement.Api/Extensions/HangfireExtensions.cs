using Hangfire;
using Hangfire.PostgreSql;

namespace MoneyManagement.Api.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddConfiguredHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection")))
            // Batch import jobs write valid rows chunk-by-chunk and are not safe to replay from
            // scratch after a partial failure, so don't let Hangfire auto-retry them.
            .UseFilter(new AutomaticRetryAttribute { Attempts = 0 }));

        services.AddHangfireServer();

        return services;
    }

    public static IApplicationBuilder UseHangfireDashboardInDevelopment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseHangfireDashboard();
        }

        return app;
    }
}
