#!/usr/bin/env pwsh

<#
.SYNOPSIS
Deploys Azure resources for PoTicTac application
.DESCRIPTION
This script deploys all required Azure resources using Bicep templates
#>

param(
    [string]$EnvironmentName = "PoTicTac",
    [string]$Location = "eastus"
)

$ErrorActionPreference = "Stop"

Write-Host "Starting Azure deployment for $EnvironmentName..." -ForegroundColor Green

# Check if logged in to Azure
Write-Host "Checking Azure login status..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "Not logged in to Azure. Please login..." -ForegroundColor Yellow
    az login
}

$account = az account show | ConvertFrom-Json
Write-Host "Using subscription: $($account.name) ($($account.id))" -ForegroundColor Green

# Deploy infrastructure
Write-Host "`nDeploying infrastructure..." -ForegroundColor Yellow
$deploymentName = "PoTicTac-$(Get-Date -Format 'yyyyMMddHHmmss')"

$deployment = az deployment sub create `
    --name $deploymentName `
    --location $Location `
    --template-file "./infra/main.bicep" `
    --parameters "./infra/main.bicepparam" `
    --parameters environmentName=$EnvironmentName `
    --query "properties.outputs" `
    --output json | ConvertFrom-Json

if ($LASTEXITCODE -ne 0) {
    Write-Error "Deployment failed"
    exit 1
}

Write-Host "`nDeployment completed successfully!" -ForegroundColor Green

# Extract outputs
$resourceGroup = $deployment.AZURE_RESOURCE_GROUP.value
$appInsightsConnectionString = $deployment.APPLICATIONINSIGHTS_CONNECTION_STRING.value
$storageConnectionString = $deployment.AZURE_STORAGE_CONNECTION_STRING.value
$storageAccountName = $deployment.AZURE_STORAGE_ACCOUNT_NAME.value

Write-Host "`n========== Deployment Outputs ==========" -ForegroundColor Cyan
Write-Host "Resource Group: $resourceGroup" -ForegroundColor White
Write-Host "Storage Account: $storageAccountName" -ForegroundColor White
Write-Host "========================================`n" -ForegroundColor Cyan

# Update appsettings.json with connection strings
$appSettingsPath = "./PoTicTacServer/appsettings.json"
if (Test-Path $appSettingsPath) {
    Write-Host "Updating appsettings.json..." -ForegroundColor Yellow
    $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
    
    if (-not $appSettings.ApplicationInsights) {
        $appSettings | Add-Member -NotePropertyName "ApplicationInsights" -NotePropertyValue @{} -Force
    }
    $appSettings.ApplicationInsights.ConnectionString = $appInsightsConnectionString
    
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
    Write-Host "Updated appsettings.json" -ForegroundColor Green
}

# Create .env file for local development reference
$envContent = @"
# Azure Resource Configuration
AZURE_RESOURCE_GROUP=$resourceGroup
AZURE_LOCATION=$Location
AZURE_STORAGE_ACCOUNT_NAME=$storageAccountName

# For local development, use Azurite
# AZURE_STORAGE_CONNECTION_STRING=UseDevelopmentStorage=true

# For Azure deployment
AZURE_STORAGE_CONNECTION_STRING=$storageConnectionString
APPLICATIONINSIGHTS_CONNECTION_STRING=$appInsightsConnectionString
"@

$envContent | Out-File -FilePath ".env" -Encoding UTF8
Write-Host "`nCreated .env file with connection strings" -ForegroundColor Green

Write-Host "`n✅ Deployment complete!" -ForegroundColor Green
Write-Host "Local development uses Azurite (configured in appsettings.Development.json)" -ForegroundColor Cyan
Write-Host "Azure deployment uses the connection strings above" -ForegroundColor Cyan
