param location string
param resourceName string
param principalId string = ''
param useExistingAppServicePlan bool = false
param existingAppServicePlanName string = 'PoShared'
param existingAppServicePlanResourceGroup string = 'PoShared'
param deployAppService bool = false

// Log Analytics Workspace for Application Insights
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: resourceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018' // Pay-as-you-go (cheapest option)
    }
    retentionInDays: 30 // Minimum retention
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: resourceName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Storage Account for Table Storage
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: toLower(replace(resourceName, '-', '')) // Storage account names must be lowercase and no hyphens
  location: location
  sku: {
    name: 'Standard_LRS' // Cheapest option - Locally Redundant Storage
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
    accessTier: 'Hot'
  }
}

// Table Service (part of Storage Account)
resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

// Create the PlayerStats table
resource playerStatsTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-01-01' = {
  parent: tableService
  name: 'PlayerStats'
}

// RBAC role assignment for storage (if principalId is provided)
resource storageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(principalId)) {
  name: guid(storageAccount.id, principalId, 'Storage Table Data Contributor')
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3') // Storage Table Data Contributor
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

// Reference existing shared App Service Plan (F1 Free Tier) from another resource group
// Azure only allows one Free tier plan per subscription
resource existingAppServicePlan 'Microsoft.Web/serverfarms@2023-01-01' existing = {
  name: existingAppServicePlanName
  scope: resourceGroup(existingAppServicePlanResourceGroup)
}

var appServicePlanId = useExistingAppServicePlan 
  ? existingAppServicePlan.id
  : newAppServicePlan.id

// Create new App Service Plan if not using existing
resource newAppServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = if (!useExistingAppServicePlan) {
  name: resourceName
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
    size: 'F1'
    family: 'F'
    capacity: 1
  }
  properties: {
    reserved: false // false = Windows, true = Linux
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2023-01-01' = if (deployAppService) {
  name: resourceName
  location: location
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      use32BitWorkerProcess: true // Required for F1 tier
      alwaysOn: false // Must be false for F1 tier
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      healthCheckPath: '/health'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
      ]
      connectionStrings: [
        {
          name: 'AZURE_STORAGE_CONNECTION_STRING'
          connectionString: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
          type: 'Custom'
        }
      ]
    }
  }
}

// Outputs
output applicationInsightsConnectionString string = appInsights.properties.ConnectionString
output applicationInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
output logAnalyticsWorkspaceId string = logAnalytics.id
output storageAccountName string = storageAccount.name
@secure()
output storageConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
output appServiceName string = deployAppService ? appService!.name : ''
output appServiceUrl string = deployAppService ? 'https://${appService!.properties.defaultHostName}' : ''
