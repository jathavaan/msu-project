// New Consumption-plan Function App hosting the coupon reminder and budget alert timer functions —
// issue #54, did not exist before this template. This provisions the empty host only; the function
// code itself is application work tracked separately.
@description('Location for the plan and function app.')
param location string

@description('Name of the dedicated Functions storage account (storage-functions.bicep).')
param functionsStorageAccountName string

@description('Connection string of the workspace-based Application Insights resource.')
param appInsightsConnectionString string

@description('Resource ID of the financeone-uami identity used to reach Key Vault / Storage / ACS.')
param uamiResourceId string

@description('Client ID of the financeone-uami identity, needed by the runtime to pick the right identity when more than one is assigned.')
param uamiClientId string

@description('Principal ID of the financeone-uami identity, used for the storage RBAC grants the identity-based AzureWebJobsStorage connection requires.')
param uamiPrincipalId string

@description('Key Vault URI, passed through so the Function App can resolve Key Vault references the same way the API does.')
param keyVaultUri string

resource functionsStorage 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: functionsStorageAccountName
}

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'financeone-functions-plan'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  kind: 'functionapp'
  properties: {}
}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: 'financeone-functions'
  location: location
  kind: 'functionapp'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${uamiResourceId}': {}
    }
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          // Identity-based connection (no account key), matching the AAD-only pattern used for
          // MySQL and Key Vault elsewhere in this project. Requires the RBAC grants below.
          name: 'AzureWebJobsStorage__accountName'
          value: functionsStorage.name
        }
        {
          name: 'AzureWebJobsStorage__credential'
          value: 'managedidentity'
        }
        {
          name: 'AzureWebJobsStorage__clientId'
          value: uamiClientId
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: uamiClientId
        }
        {
          name: 'KeyVault__Uri'
          value: keyVaultUri
        }
      ]
    }
  }
}

var storageBlobDataOwnerRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')
var storageQueueDataContributorRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '974c5e8b-45b9-4653-ba55-5f855dd0fb88')
var storageTableDataContributorRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3')

resource blobDataOwner 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(functionsStorage.id, uamiPrincipalId, storageBlobDataOwnerRoleId)
  scope: functionsStorage
  properties: {
    principalId: uamiPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageBlobDataOwnerRoleId
  }
}

resource queueDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(functionsStorage.id, uamiPrincipalId, storageQueueDataContributorRoleId)
  scope: functionsStorage
  properties: {
    principalId: uamiPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageQueueDataContributorRoleId
  }
}

resource tableDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(functionsStorage.id, uamiPrincipalId, storageTableDataContributorRoleId)
  scope: functionsStorage
  properties: {
    principalId: uamiPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageTableDataContributorRoleId
  }
}

output id string = functionApp.id
output name string = functionApp.name
