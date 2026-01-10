using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Po.TicTac.Api;
using Azure.Data.Tables;

namespace PoTicTac.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that configures the API for integration tests
/// using Azurite storage emulator (running in Docker).
/// </summary>
public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific configuration
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:tables"] = "UseDevelopmentStorage=true"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing TableServiceClient registration from Aspire
            services.RemoveAll<TableServiceClient>();

            // Add TableServiceClient configured for Azurite
            services.AddSingleton(new TableServiceClient("UseDevelopmentStorage=true"));
        });
    }
}
