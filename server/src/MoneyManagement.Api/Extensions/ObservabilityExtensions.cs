using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

namespace MoneyManagement.Api.Extensions;

public static class ObservabilityExtensions
{
    private static readonly string[] QuietPaths = ["/health", "/metrics"];

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection("Observability");
        var service = section["ServiceName"] ?? "finora-api";
        var lokiUrl = section["LokiUrl"];
        var otlpEndpoint = section["OtlpEndpoint"];

        builder.Host.UseSerilog((ctx, logger) =>
        {
            logger
                .ReadFrom.Configuration(ctx.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("app", service)
                .Enrich.WithProperty("env", ctx.HostingEnvironment.EnvironmentName)
                .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(lokiUrl))
            {
                logger.WriteTo.GrafanaLoki(lokiUrl, labels:
                [
                    new LokiLabel { Key = "app", Value = service },
                    new LokiLabel { Key = "env", Value = ctx.HostingEnvironment.EnvironmentName }
                ]);
            }
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(service))
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("Npgsql")
                .AddPrometheusExporter())
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation(o => o.Filter = http => !IsQuiet(http.Request.Path))
                    .AddHttpClientInstrumentation()
                    .AddNpgsql();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    t.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
                }
            });

        return builder;
    }

    public static WebApplication UseRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(o => o.GetLevel = (http, _, ex) =>
            ex is not null || http.Response.StatusCode >= 500 ? LogEventLevel.Error
            : IsQuiet(http.Request.Path) ? LogEventLevel.Verbose
            : LogEventLevel.Information);
        return app;
    }

    public static WebApplication MapAppMetrics(this WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint("/metrics");
        return app;
    }

    private static bool IsQuiet(PathString path) => QuietPaths.Any(p => path.StartsWithSegments(p));
}
