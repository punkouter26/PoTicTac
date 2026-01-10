using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using PoTicTac.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure HttpClient with the host's base address
// In Blazor Web App, HostEnvironment.BaseAddress correctly resolves to the server URL
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

// Add FluentUI Blazor services
builder.Services.AddFluentUIComponents();

// Register game services
builder.Services.AddScoped<GameLogicService>();
builder.Services.AddScoped<AILogicService>();
builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<StatisticsService>();

await builder.Build().RunAsync();
