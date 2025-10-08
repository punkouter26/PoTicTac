using Xunit;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using FluentAssertions;

namespace PoTicTac.IntegrationTests
{
    public class AzureResourceTests : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly TableServiceClient _tableServiceClient;

        public AzureResourceTests()
        {
            // Build configuration from appsettings
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.Development.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = _configuration.GetConnectionString("AZURE_STORAGE_CONNECTION_STRING");
            _tableServiceClient = new TableServiceClient(connectionString);
        }

        [Fact]
        public async Task TableStorage_ConnectionTest_ShouldSucceed()
        {
            // Arrange
            var testTableName = "TestTable";
            var tableClient = _tableServiceClient.GetTableClient(testTableName);

            try
            {
                // Act
                var response = await tableClient.CreateIfNotExistsAsync();

                // Assert - If table was created or already exists, this should succeed
                tableClient.Should().NotBeNull("Table client should be created successfully");
                
                // Verify we can query the table
                await foreach (var entity in tableClient.QueryAsync<TableEntity>(maxPerPage: 1))
                {
                    // Just checking we can query - no need to enumerate
                    break;
                }
            }
            finally
            {
                // Cleanup
                try { await tableClient.DeleteAsync(); } catch { }
            }
        }

        [Fact]
        public async Task TableStorage_PlayerStatsTable_ShouldExist()
        {
            // Arrange
            var tableClient = _tableServiceClient.GetTableClient("PlayerStats");

            // Act
            await tableClient.CreateIfNotExistsAsync();

            // Assert - If table was created or already exists, this should succeed
            tableClient.Should().NotBeNull("PlayerStats table client should be created successfully");
            
            // Verify we can query the table
            await foreach (var entity in tableClient.QueryAsync<TableEntity>(maxPerPage: 1))
            {
                // Just checking we can query - no need to enumerate
                break;
            }
        }

        [Fact]
        public async Task TableStorage_CRUD_Operations_ShouldWork()
        {
            // Arrange
            var testTableName = "TestCRUDTable";
            var tableClient = _tableServiceClient.GetTableClient(testTableName);
            
            try
            {
                await tableClient.CreateIfNotExistsAsync();

                var entity = new TableEntity("TestPartition", "TestRow")
                {
                    { "Name", "TestValue" },
                    { "Count", 42 }
                };

                // Act & Assert - Create
                var addResponse = await tableClient.AddEntityAsync(entity);
                addResponse.Should().NotBeNull();

                // Act & Assert - Read
                var getResponse = await tableClient.GetEntityAsync<TableEntity>("TestPartition", "TestRow");
                getResponse.Value.Should().NotBeNull();
                getResponse.Value.GetString("Name").Should().Be("TestValue");
                getResponse.Value.GetInt32("Count").Should().Be(42);

                // Act & Assert - Update
                entity["Count"] = 100;
                await tableClient.UpdateEntityAsync(entity, getResponse.Value.ETag);
                var updatedEntity = await tableClient.GetEntityAsync<TableEntity>("TestPartition", "TestRow");
                updatedEntity.Value.GetInt32("Count").Should().Be(100);

                // Act & Assert - Delete
                await tableClient.DeleteEntityAsync("TestPartition", "TestRow");
            }
            finally
            {
                // Cleanup
                await tableClient.DeleteAsync();
            }
        }

        [Fact]
        public void ApplicationInsights_Configuration_ShouldBePresent()
        {
            // Arrange & Act
            var appInsightsConnectionString = _configuration["ApplicationInsights:ConnectionString"];

            // Assert
            // Connection string can be empty for local development
            appInsightsConnectionString.Should().NotBeNull("ApplicationInsights configuration should be present");
        }

        [Fact]
        public async Task TableStorage_ListTables_ShouldNotThrow()
        {
            // Act
            Func<Task> act = async () =>
            {
                await foreach (var table in _tableServiceClient.QueryAsync())
                {
                    // Just enumerate to test connectivity
                    _ = table.Name;
                    break; // Only check first one
                }
            };

            // Assert
            await act.Should().NotThrowAsync("Listing tables should work with valid connection");
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
