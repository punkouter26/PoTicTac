targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment (used for resource group and resource naming)')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@allowed([
  'dev'
  'prod'
])
@description('Environment type - determines App Service Plan selection')
param environmentType string = 'dev'

@description('Service principal ID for RBAC assignments (optional)')
param principalId string = ''

@description('Tags to apply to all resources')
param tags object = {
  environment: environmentType
  application: 'PoTicTac'
}

// Resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: environmentName
  location: location
  tags: tags
}

// Get the existing App Service Plan ID from PoShared resource group (PoShared is in East US 2, closest to our East US deployment)
var appServicePlanId = resourceId(subscription().subscriptionId, 'PoShared', 'Microsoft.Web/serverfarms', 'PoShared')

// Deploy resources
module resources './resources.bicep' = {
  name: 'resources'
  scope: rg
  params: {
    location: location
    resourceName: environmentName
    environmentType: environmentType
    principalId: principalId
    tags: tags
    appServicePlanId: appServicePlanId
  }
}

// Outputs
output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_KEY_VAULT_ENDPOINT string = resources.outputs.keyVaultEndpoint
output APPLICATIONINSIGHTS_CONNECTION_STRING string = resources.outputs.applicationInsightsConnectionString
output AZURE_STORAGE_ACCOUNT_NAME string = resources.outputs.storageAccountName
output APP_SERVICE_NAME string = resources.outputs.appServiceName
output APP_SERVICE_URL string = resources.outputs.appServiceUrl
output APP_SERVICE_PRINCIPAL_ID string = resources.outputs.appServicePrincipalId
