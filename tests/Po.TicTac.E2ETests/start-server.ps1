#!/usr/bin/env pwsh
# Start the API server for E2E tests
$env:ASPNETCORE_ENVIRONMENT = 'Development'
Set-Location "$PSScriptRoot\..\.."
dotnet run --project src/Po.TicTac.Api/Po.TicTac.Api.csproj --no-launch-profile
