@description('The location used for all deployed resources')
param location string = resourceGroup().location
@description('Id of the user or app to assign application roles')
param principalId string = ''

@description('Tags that will be applied to all resources')
param tags object = {}

// Reference existing shared resources in PoShared resource group
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: 'acrposhared'
  scope: resourceGroup('PoShared')
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: 'law-poshared'
  scope: resourceGroup('PoShared')
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2024-02-02-preview' existing = {
  name: 'cae-poshared'
  scope: resourceGroup('PoShared')
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: 'kv-poshared'
  scope: resourceGroup('PoShared')
}

// Create managed identity in this resource group for the app
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-potictac'
  location: location
  tags: tags
}

output MANAGED_IDENTITY_CLIENT_ID string = managedIdentity.properties.clientId
output MANAGED_IDENTITY_NAME string = managedIdentity.name
output MANAGED_IDENTITY_PRINCIPAL_ID string = managedIdentity.properties.principalId
output AZURE_LOG_ANALYTICS_WORKSPACE_NAME string = 'law-poshared'
output AZURE_LOG_ANALYTICS_WORKSPACE_ID string = '/subscriptions/${subscription().subscriptionId}/resourceGroups/PoShared/providers/Microsoft.OperationalInsights/workspaces/law-poshared'
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = 'acrposhared.azurecr.io'
output AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = managedIdentity.id
output AZURE_CONTAINER_REGISTRY_NAME string = 'acrposhared'
output AZURE_CONTAINER_APPS_ENVIRONMENT_NAME string = 'cae-poshared'
output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = '/subscriptions/${subscription().subscriptionId}/resourceGroups/PoShared/providers/Microsoft.App/managedEnvironments/cae-poshared'
output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = 'braveground-e6b1356c.eastus.azurecontainerapps.io'
output AZURE_KEY_VAULT_URI string = 'https://kv-poshared.vault.azure.net/'
