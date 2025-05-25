targetScope = 'resourceGroup'

@minLength(1)
@maxLength(64)
@description('Name of the environment which is used to generate a short unique hash for resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Flag to use existing shared resources or create new ones')
param useExistingSharedResources bool = false

@description('Name of existing shared App Service Plan (when useExistingSharedResources is true)')
param existingAppServicePlanName string = 'PoSharedAppServicePlan'

@description('Name of existing shared Application Insights (when useExistingSharedResources is true)')
param existingApplicationInsightsName string = 'PoSharedApplicationInsights'

@description('Name of existing shared resource group (when useExistingSharedResources is true)')
param existingSharedResourceGroupName string = 'PoShared'

// Generate a unique suffix for resources
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var tags = { 'azd-env-name': environmentName }
var appName = 'potictac-${resourceToken}'

// Create or reference App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = if (!useExistingSharedResources) {
  name: '${appName}-plan'
  location: location
  tags: tags
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  properties: {
    reserved: false
  }
}

// Reference existing shared App Service Plan
resource existingAppServicePlan 'Microsoft.Web/serverfarms@2023-01-01' existing = if (useExistingSharedResources) {
  name: existingAppServicePlanName
  scope: resourceGroup(existingSharedResourceGroupName)
}

// Create or reference Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = if (!useExistingSharedResources) {
  name: '${appName}-insights'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

// Reference existing shared Application Insights
resource existingAppInsights 'Microsoft.Insights/components@2020-02-02' existing = if (useExistingSharedResources) {
  name: existingApplicationInsightsName
  scope: resourceGroup(existingSharedResourceGroupName)
}

// Create App Service
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: appName
  location: location
  tags: tags
  properties: {
    serverFarmId: useExistingSharedResources ? existingAppServicePlan.id : appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      appSettings: [
        {
          name: 'ConnectionStrings__AZURE_STORAGE_CONNECTION_STRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${tableStorage.name};AccountKey=${tableStorage.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'ApplicationInsights__ConnectionString'
          value: useExistingSharedResources ? existingAppInsights.properties.ConnectionString : applicationInsights.properties.ConnectionString
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
    }
  }
}

// Create Table Storage Account
resource tableStorage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'st${resourceToken}'
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    accessTier: 'Hot'
  }
}

// Enable Table service
resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  parent: tableStorage
  name: 'default'
}

// Outputs for AZD
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output WEBSITE_HOSTNAME string = appService.properties.defaultHostName
output WEB_URI string = 'https://${appService.properties.defaultHostName}'
