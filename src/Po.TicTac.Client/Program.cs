using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using PoTicTac.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add FluentUI Blazor services
builder.Services.AddFluentUIComponents();

// Register game services
builder.Services.AddScoped<GameLogicService>();
builder.Services.AddScoped<AILogicService>();
builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<StatisticsService>();

await builder.Build().RunAsync();
