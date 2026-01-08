var builder = DistributedApplication.CreateBuilder(args);

// Add Azure Table Storage for player stats persistence
// Uses Azurite emulator locally, real Azure Storage in production
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator => emulator
        .WithDataVolume("potictac-storage")
        .WithLifetime(ContainerLifetime.Persistent));

var tables = storage.AddTables("tables");

// Add the API service which hosts the Blazor WASM client
builder.AddProject<Projects.Po_TicTac_Api>("api")
    .WithHttpEndpoint(port: 5000, name: "http")
    .WithHttpsEndpoint(port: 5001, name: "https")
    .WithExternalHttpEndpoints()
    .WithReference(tables)
    .WaitFor(tables);

builder.Build().Run();
