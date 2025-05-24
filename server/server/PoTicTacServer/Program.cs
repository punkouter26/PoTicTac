using PoTicTacServer.Services;
using PoTicTacServer.Hubs;
using Serilog;
using PoTicTacServer.HealthChecks;
using Azure.Data.Tables;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    var loggerConfiguration = new LoggerConfiguration()
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(Directory.GetCurrentDirectory(), "log.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 1,
            flushToDiskInterval: TimeSpan.FromSeconds(1)
        );

    // Conditionally add Application Insights sink
    if (!string.IsNullOrWhiteSpace(context.Configuration["ApplicationInsights:ConnectionString"]))
    {
        loggerConfiguration.WriteTo.ApplicationInsights(
            context.Configuration["ApplicationInsights:ConnectionString"],
            TelemetryConverter.Traces
        );
    }

    loggerConfiguration
        .MinimumLevel.Override("Microsoft.ApplicationInsights", Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning);

    Log.Logger = loggerConfiguration.CreateLogger();
});

// Add services to the container.
builder.Services.AddSingleton<StorageService>();

// Register TableServiceClient for DI
builder.Services.AddSingleton(x => new TableServiceClient(builder.Configuration.GetConnectionString("AZURE_STORAGE_CONNECTION_STRING")));

// Add SignalR services
builder.Services.AddSignalR();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<StorageHealthCheck>("AzureTableStorage");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(); // Add this to log HTTP requests

app.UseHttpsRedirection();
app.UseAuthorization();

// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapControllers();

// Map SignalR hub
app.MapHub<GameHub>("/gamehub");

// Map Health Checks endpoint
app.MapHealthChecks("/health");

// Fallback to index.html for SPA routes
app.MapFallbackToFile("index.html");

app.Run();
