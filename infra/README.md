# Azure Infrastructure for PoTicTac

This document describes the Azure infrastructure setup for the PoTicTac application.

## Azure Resources

The application uses the following Azure resources (all in resource group: **PoTicTac**):

1. **Log Analytics Workspace** (`PoTicTac`)
   - SKU: PerGB2018 (Pay-as-you-go)
   - Retention: 30 days (minimum)
   - Purpose: Backend for Application Insights

2. **Application Insights** (`PoTicTac`)
   - Type: Web application monitoring
   - Purpose: Telemetry, logging, and performance monitoring

3. **Storage Account** (`potictac`)
   - SKU: Standard_LRS (Cheapest - Locally Redundant Storage)
   - Kind: StorageV2
   - Purpose: Azure Table Storage for player statistics
   - Table: `PlayerStats`

## Local Development

For local development, the application uses **Azurite** (Azure Storage Emulator):

```json
"ConnectionStrings": {
  "AZURE_STORAGE_CONNECTION_STRING": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;..."
}
```

This is configured in `appsettings.Development.json`.

### Starting Azurite

Azurite should already be running if you're developing locally. The connection string is pre-configured.

## Deployment

### Prerequisites

- Azure CLI installed and logged in (`az login`)
- .NET 9.0 SDK installed
- PowerShell (for deployment scripts)

### Deploy Infrastructure

Run the deployment script to create all Azure resources:

```powershell
.\scripts\deploy-azure.ps1
```

This script will:
1. Create the resource group `PoTicTac` in East US
2. Deploy Log Analytics Workspace
3. Deploy Application Insights
4. Deploy Storage Account with PlayerStats table
5. Update `appsettings.json` with connection strings
6. Create a `.env` file with all connection strings

### Manual Deployment (using Azure CLI)

If you prefer manual deployment:

```powershell
# Deploy using Bicep
az deployment sub create \
  --name PoTicTac-deployment \
  --location eastus \
  --template-file ./infra/main.bicep \
  --parameters ./infra/main.bicepparam
```

### Cleanup

To delete all Azure resources:

```powershell
.\scripts\cleanup-azure.ps1
```

Or manually:

```powershell
az group delete --name PoTicTac --yes
```

## Configuration

### Local Development (appsettings.Development.json)
- Uses Azurite for Table Storage
- Application Insights connection string can be empty

### Azure Production (appsettings.json)
- Uses Azure Table Storage
- Application Insights connection string is configured during deployment

## Integration Tests

Azure resource connectivity tests are located in:
- `PoTicTac.IntegrationTests/AzureResourceTests.cs`

Run tests to verify Azure connectivity:

```powershell
dotnet test PoTicTac.IntegrationTests
```

The tests verify:
- Table Storage connection
- CRUD operations on tables
- PlayerStats table existence
- Configuration presence

## Resource Naming Convention

All resources use the same name: **PoTicTac**
- Resource Group: `PoTicTac`
- Log Analytics: `PoTicTac`
- Application Insights: `PoTicTac`
- Storage Account: `potictac` (lowercase, no hyphens)

## Cost Optimization

All resources are configured with the cheapest/free tier options:
- **Log Analytics**: Pay-as-you-go with 30-day retention
- **Application Insights**: Free tier with Log Analytics workspace
- **Storage**: Standard_LRS (Locally Redundant Storage)
- **Table Storage**: Pay-per-transaction (minimal cost)

Estimated monthly cost: < $5 USD for minimal usage

## Troubleshooting

### Connection String Issues

If you get connection errors, verify:
1. Azurite is running for local development
2. Connection strings in `appsettings.Development.json` are correct
3. For Azure: Run `.\scripts\deploy-azure.ps1` to ensure resources exist

### Table Storage Access

If you can't access tables:
1. Ensure the table exists (created automatically on first use)
2. Check connection string format
3. Verify firewall rules (Azure Storage)

## Security Notes

- `.env` file is in `.gitignore` and should never be committed
- Connection strings contain secrets - keep them secure
- For production, use Azure Key Vault or Managed Identity instead of connection strings
