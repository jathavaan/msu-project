// New Consumption-plan Function App hosting the coupon reminder and budget alert timer functions —
// issue #54, did not exist before this template. This provisions the empty host only; the function
// code itself is application work tracked separately.
//
// AzureWebJobsStorage below uses an identity-based connection (no account key). The RBAC grants it
// needs (Storage Blob Data Owner / Queue Data Contributor / Table Data Contributor on the Functions
// storage account) live in ../role-assignments.bicep, not here — see that file for why. Until that
// template is applied by hand, the Function App will exist but fail to start.
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

output id string = functionApp.id
output name string = functionApp.name
