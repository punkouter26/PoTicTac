using Azure.Data.Tables;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Po.TicTac.Api.Features.Health;
using Po.TicTac.Api.Features.Players;
using Po.TicTac.Api.Features.Statistics;
using Po.TicTac.Api.HealthChecks;
using Po.TicTac.Api.Hubs;
using Po.TicTac.Api.Services;
using Po.TicTac.Api.Telemetry;
using Serilog;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

var builder = WebApplication.CreateBuilder(args);

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
        // Production: write to Application Insights
        var aiConnectionString = context.Configuration["ApplicationInsights:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(aiConnectionString))
        {
            configuration
                .WriteTo.ApplicationInsights(
                    aiConnectionString,
                    new TraceTelemetryConverter(),
                    Serilog.Events.LogEventLevel.Information
                );
        }

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

// Configure Application Insights
var aiConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrWhiteSpace(aiConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = aiConnectionString;
        options.EnableAdaptiveSampling = builder.Configuration.GetValue<bool>("ApplicationInsights:EnableAdaptiveSampling", true);
        options.EnablePerformanceCounterCollectionModule = builder.Configuration.GetValue<bool>("ApplicationInsights:EnablePerformanceCounterCollectionModule", true);
        options.EnableQuickPulseMetricStream = builder.Configuration.GetValue<bool>("ApplicationInsights:EnableQuickPulseMetricStream", true);
    });

    // Register custom telemetry initializer
    builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();

    // Enable Snapshot Debugger if configured
    if (builder.Configuration.GetValue<bool>("ApplicationInsights:EnableSnapshotDebugger", false))
    {
        builder.Services.AddSnapshotCollector(options =>
        {
            options.IsEnabledInDeveloperMode = false;
            options.ThresholdForSnapshotting = builder.Configuration.GetValue<int>("SnapshotDebugger:ThresholdForSnapshotting", 1);
            options.MaximumSnapshotsRequired = builder.Configuration.GetValue<int>("SnapshotDebugger:MaximumSnapshotsRequired", 3);
        });
    }

    // Enable Profiler if configured
    if (builder.Configuration.GetValue<bool>("ApplicationInsights:EnableProfiler", false))
    {
        builder.Services.AddServiceProfiler();
    }
}

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add services to the container.
// Register TableServiceClient and TableClient for DI
builder.Services.AddSingleton(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("AZURE_STORAGE_CONNECTION_STRING") ?? "UseDevelopmentStorage=true";
    var tableServiceClient = new TableServiceClient(connectionString);
    return tableServiceClient.GetTableClient("PlayerStats");
});

builder.Services.AddSingleton<StorageService>();


// Add SignalR services
builder.Services.AddSignalR();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<StorageHealthCheck>("AzureTableStorage");

// Add API Explorer and Swagger for Minimal APIs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PoTicTac API",
        Version = "v1",
        Description = "RESTful API for PoTicTac - A modern, retro-styled 6x6 Tic Tac Toe game with 4-in-a-row victory conditions. " +
                      "Provides endpoints for player statistics, leaderboards, health checks, and game data management.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "PoTicTac Development Team",
            Url = new Uri("https://github.com/punkouter26/PoTicTac")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments for better API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for API testing
app.UseSwagger();
app.UseSwaggerUI();

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

// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// Map Minimal API endpoints
app.MapHealthCheck();
app.MapGetAllPlayerStatistics();
app.MapGetLeaderboard();
app.MapGetPlayerStats();
app.MapSavePlayerStats();

// Map SignalR hub
app.MapHub<GameHub>("/gamehub");

// Fallback to index.html for SPA routes (only for non-API routes)
app.MapFallbackToFile("index.html").ExcludeFromDescription();

Log.Information("PoTicTac server starting on {Environment}", builder.Environment.EnvironmentName);

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
