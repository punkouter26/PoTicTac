using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Po.TicTac.Api;
using Azure.Data.Tables;
using Testcontainers.Azurite;

namespace PoTicTac.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that configures the API for integration tests
/// using Azurite Testcontainer for Azure Table Storage emulation.
/// </summary>
public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
        .Build();

    public async Task InitializeAsync()
    {
        await _azuriteContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific configuration using Azurite connection string
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Tables"] = _azuriteContainer.GetConnectionString(),
                // Disable App Insights for tests
                ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = null
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing TableServiceClient registration
            services.RemoveAll<TableServiceClient>();

            // Add TableServiceClient configured for Azurite Testcontainer
            services.AddSingleton(new TableServiceClient(_azuriteContainer.GetConnectionString()));
        });
    }
}
