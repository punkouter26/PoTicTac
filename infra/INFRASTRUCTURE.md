# PoTicTac Azure Infrastructure

This directory contains the Bicep templates for provisioning Azure resources for the PoTicTac application.

## Overview

The infrastructure is designed to be deployed using Azure Developer CLI (`azd`) with zero user input required during deployment.

## Architecture

### Core Resources

1. **Resource Group**: `{environmentName}-rg`
2. **Log Analytics Workspace**: `{environmentName}-logs` - Centralized logging for all resources
3. **Application Insights**: `{environmentName}-ai` - Application performance monitoring
4. **Storage Account**: `{environmentName}st` - Azure Table Storage for player statistics
5. **Key Vault**: `{environmentName}-kv` - Secure storage for secrets and connection strings
6. **App Service Plan**: 
   - Dev: Uses existing shared F1 plan from `PoShared` resource group
   - Prod: Creates dedicated B1 plan
7. **App Service**: `{environmentName}-app` (when deployed)

### Environment-Based Configuration

The infrastructure uses the `environmentType` parameter to determine resource configuration:

- **dev**: Uses shared F1 (Free) App Service Plan, optimized for cost
- **prod**: Creates dedicated B1 (Basic) App Service Plan for performance and isolation

## Deployment

### Prerequisites

- Azure CLI (`az`) installed and authenticated
- Azure Developer CLI (`azd`) installed
- Appropriate Azure subscription permissions

### Deploy Infrastructure Only

```bash
# Initialize azd environment (first time only)
azd init

# Provision infrastructure without deploying code
azd provision
```

### Full Deployment (Infrastructure + Code)

```bash
# Deploy everything
azd up
```

## Parameters

Parameters are defined in `main.bicepparam`:

- `environmentName`: Name prefix for all resources (default: 'PoTicTac')
- `location`: Azure region (default: 'eastus')
- `environmentType`: Environment type - 'dev' or 'prod' (default: 'dev')

## Security Features

### Managed Identity

The App Service is configured with a **System-Assigned Managed Identity** that is granted:

- **Key Vault Secrets User** role on the Key Vault
- **Storage Table Data Contributor** role on the Storage Account

### Key Vault Integration

Sensitive configuration values are stored in Azure Key Vault:

- Storage account connection strings
- Any future secrets

The App Service accesses these secrets using Key Vault References in Application Settings.

### Network Security

- All resources enforce HTTPS/TLS 1.2 minimum
- Storage Account has public blob access disabled
- Key Vault uses RBAC authorization

## Diagnostic Settings

### App Service Diagnostics

All App Service logs are forwarded to Log Analytics:
- HTTP Logs
- Console Logs
- Application Logs
- Audit Logs
- IP Security Audit Logs
- Platform Logs

### Storage Account Diagnostics

Storage metrics and Table Service logs are forwarded to Log Analytics:
- Transaction metrics
- Read/Write/Delete operations

## Local vs Cloud Development

### Local Development
- Uses **Azurite** storage emulator
- Connection string in `appsettings.Development.json`
- No Azure resources required

### Cloud Deployment
- Uses **Azure Storage Account** provisioned by Bicep
- Connection string retrieved from Key Vault via Managed Identity
- Application Insights enabled for telemetry

## Resource Naming Convention

All resources follow the pattern: `{environmentName}-{suffix}`

Examples:
- Resource Group: `PoTicTac-rg`
- App Service: `PoTicTac-app`
- Storage: `potictacst` (lowercase, no hyphens)
- Key Vault: `PoTicTac-kv`

## Table Storage

The infrastructure provisions a table named `PoTicTacPlayerStats` in the storage account.

## Outputs

The deployment provides the following outputs:

- `AZURE_LOCATION`: Deployment location
- `AZURE_RESOURCE_GROUP`: Resource group name
- `AZURE_KEY_VAULT_ENDPOINT`: Key Vault URI
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: Application Insights connection string
- `AZURE_STORAGE_ACCOUNT_NAME`: Storage account name
- `APP_SERVICE_NAME`: App Service name (when deployed)
- `APP_SERVICE_URL`: App Service URL (when deployed)

## Cost Optimization

### Development Environment
- Uses shared F1 (Free) App Service Plan
- Standard LRS storage (cheapest option)
- 30-day log retention

### Production Environment
- Dedicated B1 App Service Plan ($~13/month)
- Can scale up/out as needed
- Same storage and logging configuration as dev

## Troubleshooting

### Shared App Service Plan Not Found

If deployment fails because the shared F1 plan doesn't exist:

1. Set `environmentType` to 'prod' to create a dedicated plan
2. OR create the shared plan in `PoShared` resource group first

### Key Vault Access Issues

Ensure the App Service Managed Identity has been granted the proper roles. This happens automatically during deployment.
