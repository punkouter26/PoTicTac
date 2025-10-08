targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment (used for resource group and resource naming)')
param environmentName string = 'PoTicTac'

@minLength(1)
@description('Primary location for all resources')
param location string = 'eastus2'

// Service principal ID for RBAC assignments (optional)
param principalId string = ''

// Set to true to use existing shared App Service Plan
param useExistingAppServicePlan bool = true
param existingAppServicePlanName string = 'PoShared'
param existingAppServicePlanResourceGroup string = 'PoShared'
param deployAppService bool = false // Set to false to skip App Service deployment in Bicep

// Resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: environmentName
  location: location
}

// Deploy resources
module resources './resources.bicep' = {
  name: 'resources'
  scope: rg
  params: {
    location: location
    resourceName: environmentName
    principalId: principalId
    useExistingAppServicePlan: useExistingAppServicePlan
    existingAppServicePlanName: existingAppServicePlanName
    existingAppServicePlanResourceGroup: existingAppServicePlanResourceGroup
    deployAppService: deployAppService
  }
}

// Outputs
output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = rg.name
output APPLICATIONINSIGHTS_CONNECTION_STRING string = resources.outputs.applicationInsightsConnectionString
@secure()
output AZURE_STORAGE_CONNECTION_STRING string = resources.outputs.storageConnectionString
output AZURE_STORAGE_ACCOUNT_NAME string = resources.outputs.storageAccountName
output APP_SERVICE_NAME string = resources.outputs.appServiceName
output APP_SERVICE_URL string = resources.outputs.appServiceUrl
