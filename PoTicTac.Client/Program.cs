using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoTicTac.Client;
using PoTicTac.Client.Services;
using Radzen; // Add this using statement

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add Radzen Blazor services
builder.Services.AddRadzenComponents();

// Register game services
builder.Services.AddScoped<GameLogicService>();
builder.Services.AddScoped<AILogicService>();
builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<StatisticsService>(); // Register the new StatisticsService

await builder.Build().RunAsync();
