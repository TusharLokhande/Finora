using DotNetEnv;
using MoneyManagement.Api.Extensions;
using MoneyManagement.Api.Filters;
using MoneyManagement.Api.Middleware;
using MoneyManagement.Api.Services;
using MoneyManagement.Application;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Infrastructure;

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), $".env");
if (File.Exists(envFilePath))
{
    Env.Load(envFilePath);
}

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
builder.Services.AddOpenApi();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddDataProtection();

builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddConfiguredHangfire(builder.Configuration);
builder.Services.AddFrontendCors(builder.Configuration);

var app = builder.Build();

await app.Services.MigrateDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHangfireDashboardInDevelopment();

app.UseExceptionHandlingMiddleware();

app.UseHttpsRedirection();

app.UseFrontendCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapAppHealthChecks();

app.Run();
