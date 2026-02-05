using Azure.Data.Tables;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Po.TicTac.Api.Features.Health;
using Po.TicTac.Api.Features.Players;
using Po.TicTac.Api.Features.Statistics;
using Po.TicTac.Api.HealthChecks;
using Po.TicTac.Api.Services;
using Po.TicTac.Api.Telemetry;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault as configuration provider for centralized secrets
// Uses DefaultAzureCredential: Managed Identity in Azure, Visual Studio/CLI credentials locally
var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential(),
        new Azure.Extensions.AspNetCore.Configuration.Secrets.KeyVaultSecretManager());
}

// Configure Serilog with enrichers
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithThreadId()
        .Enrich.WithProcessId()
        .Enrich.With(new CorrelationEnricher(services.GetRequiredService<IHttpContextAccessor>()));

    // Configure sinks based on environment
    if (context.HostingEnvironment.IsDevelopment())
    {
        configuration
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Debug();
    }
    else
    {
        // Production: Write to console for container logs
        configuration.WriteTo.Console();

        // Also write to file for local diagnostics
        configuration
            .WriteTo.File(
                path: context.Configuration["Serilog:WriteTo:1:Args:path"] ?? "logs/potictac-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}"
            );
    }
});

// Add HttpContextAccessor for telemetry enrichers
builder.Services.AddHttpContextAccessor();

// Add OpenTelemetry with Azure Monitor for distributed tracing
var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}

// Add HybridCache for in-memory + distributed caching (.NET 10)
#pragma warning disable EXTEXP0018 // HybridCache is experimental in .NET 10
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(1)
    };
});
#pragma warning restore EXTEXP0018

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add Azure Table Storage client
// Priority: Key Vault secrets (via ConnectionStrings:Tables) > App Settings > Development storage
var tableConnectionString = builder.Configuration.GetConnectionString("Tables")
    ?? builder.Configuration["ConnectionStrings:AzureTableStorage"]
    ?? "UseDevelopmentStorage=true";

builder.Services.AddSingleton(sp =>
{
    // Use connection string directly (works for local Azurite, Key Vault secrets, or app settings)
    return new TableServiceClient(tableConnectionString);
});

builder.Services.AddSingleton<StorageService>();

// Add CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000", "https://localhost:3000"];

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<StorageHealthCheck>("AzureTableStorage");

// Add Authorization (required for UseAuthorization middleware)
builder.Services.AddAuthorization();

// Add API Explorer and OpenAPI/Swagger for Minimal APIs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "PoTicTac API";
        document.Info.Version = "v1";
        document.Info.Description = "RESTful API for PoTicTac - A modern, retro-styled 6x6 Tic Tac Toe game with 4-in-a-row victory conditions. " +
                      "Provides endpoints for player statistics, leaderboards, health checks, and game data management.";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable OpenAPI and Swagger UI in all environments for API testing
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PoTicTac API v1");
    options.RoutePrefix = "swagger"; // Access at /swagger
});

// Add Serilog request logging with enrichment
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.Identity.Name ?? "authenticated");
        }
    };
});

app.UseHttpsRedirection();

// Enable CORS before authorization
app.UseCors();

app.UseAuthorization();

// Map Minimal API endpoints
app.MapHealthCheck();
app.MapGetAllPlayerStatistics();
app.MapGetLeaderboard();
app.MapGetPlayerStats();
app.MapSavePlayerStats();

// Map health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/alive", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Liveness check - just returns healthy if app is running
});

Log.Information("PoTicTac API server starting on {Environment}", builder.Environment.EnvironmentName);

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
