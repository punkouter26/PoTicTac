using Azure.Identity;
using Po.TicTac.Api.Features.Health;
using Po.TicTac.Api.Features.Players;
using Po.TicTac.Api.Features.Statistics;
using Po.TicTac.Api.HealthChecks;
using Po.TicTac.Api.Hubs;
using Po.TicTac.Api.Services;
using Po.TicTac.Api.Telemetry;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault as configuration provider for centralized secrets
// Uses DefaultAzureCredential: Managed Identity in Azure, Visual Studio/CLI credentials locally
var keyVaultUri = builder.Configuration["KeyVault:Uri"] ?? "https://kv-poshared.vault.azure.net/";
builder.Configuration.AddAzureKeyVault(
    new Uri(keyVaultUri),
    new DefaultAzureCredential(),
    new Azure.Extensions.AspNetCore.Configuration.Secrets.KeyVaultSecretManager());

// Add Aspire ServiceDefaults for telemetry, health checks, and service discovery
// This configures OpenTelemetry with Azure Monitor for Application Insights
builder.AddServiceDefaults();

// Ensure static web assets (client + library content) are resolved when hosted by the API
builder.WebHost.UseStaticWebAssets();

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
        // Production: OpenTelemetry via ServiceDefaults handles Application Insights
        // Write to console for container logs (structured for Azure Monitor)
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

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add Aspire Azure Table Storage client (connection injected by AppHost)
builder.AddAzureTableServiceClient("tables");

builder.Services.AddSingleton<StorageService>();


// Add SignalR services
builder.Services.AddSignalR();

// Add Razor Components with Interactive Server (for business pages) and WebAssembly (for game)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add custom health check for Azure Table Storage (Aspire defaults already added)
builder.Services.AddHealthChecks()
    .AddCheck<StorageHealthCheck>("AzureTableStorage");

// Add API Explorer and OpenAPI for Minimal APIs
builder.Services.AddEndpointsApiExplorer();
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
// Enable OpenAPI in all environments for API testing
app.MapOpenApi();

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
app.UseAuthorization();

// Serve static files with fingerprinting support (.NET 8+)
app.MapStaticAssets();
app.UseAntiforgery();

// Map Minimal API endpoints
app.MapHealthCheck();
app.MapGetAllPlayerStatistics();
app.MapGetLeaderboard();
app.MapGetPlayerStats();
app.MapSavePlayerStats();

// Map SignalR hub
app.MapHub<GameHub>("/gamehub");

// Map Razor Components with Interactive Server (business pages) and WebAssembly (game)
app.MapRazorComponents<Po.TicTac.Api.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PoTicTac.Client.Pages.Home).Assembly);

// Map Aspire default endpoints (health checks, etc.)
app.MapDefaultEndpoints();

Log.Information("PoTicTac server starting on {Environment}", builder.Environment.EnvironmentName);

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
