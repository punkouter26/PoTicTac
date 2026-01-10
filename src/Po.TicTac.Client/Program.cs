using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using PoTicTac.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure HttpClient with the host's base address
// HostEnvironment.BaseAddress is set correctly by the Blazor Web App host
var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = baseAddress });

// Add FluentUI Blazor services
builder.Services.AddFluentUIComponents();

// Register game services
builder.Services.AddScoped<GameLogicService>();
builder.Services.AddScoped<AILogicService>();
builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<StatisticsService>();

await builder.Build().RunAsync();
