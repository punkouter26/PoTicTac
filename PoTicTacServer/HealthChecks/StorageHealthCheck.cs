using Microsoft.Extensions.Diagnostics.HealthChecks;
using Azure.Data.Tables;
using Azure;

namespace PoTicTacServer.HealthChecks
{
    public class StorageHealthCheck : IHealthCheck
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly IConfiguration _configuration;

        public StorageHealthCheck(TableServiceClient tableServiceClient, IConfiguration configuration)
        {
            _tableServiceClient = tableServiceClient;
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Attempt to get a table client for a dummy table to check connectivity
                var tableName = "HealthCheckTable";
                var tableClient = _tableServiceClient.GetTableClient(tableName);

                // Try to create the table if it doesn't exist, or ensure it exists
                await tableClient.CreateIfNotExistsAsync(cancellationToken);

                // Optionally, try to perform a simple operation like querying a non-existent entity
                // This verifies read/write permissions and connectivity
                try
                {
                    await tableClient.GetEntityAsync<TableEntity>("partitionKey", "rowKey", cancellationToken: cancellationToken);
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    // Expected for a non-existent entity, indicates connectivity is fine
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy($"Storage health check failed: {ex.Message}");
                }

                return HealthCheckResult.Healthy("Storage connection is healthy.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Storage health check failed: {ex.Message}");
            }
        }
    }
}
