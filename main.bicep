param appName string = 'potictac'
param location string = resourceGroup().location
param appServicePlanName string = 'PoSharedAppServicePlan'
param applicationInsightsName string = 'PoSharedApplicationInsights'
param sharedResourceGroupName string = 'PoShared'

// Reference existing shared App Service Plan
resource existingAppServicePlan 'Microsoft.Web/serverfarms@2022-03-01' existing = {
  name: appServicePlanName
  scope: resourceGroup(sharedResourceGroupName)
}

// Reference existing shared Application Insights
resource existingAppInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
  scope: resourceGroup(sharedResourceGroupName)
}

// Create App Service
resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: appName
  location: location
  properties: {
    serverFarmId: existingAppServicePlan.id
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
          value: existingAppInsights.properties.ConnectionString
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
  name: '${toLower(appName)}storage'
  location: location
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

output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output storageAccountName string = tableStorage.name
output resourceGroupName string = resourceGroup().name
