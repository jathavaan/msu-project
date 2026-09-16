// New storage account dedicated to the Function App's own runtime state (AzureWebJobsStorage:
// triggers, locks, deployment package). Kept separate from storage-app.bicep so app blob data and
// Functions plumbing don't share throughput/quota or lifecycle — issue #54, did not exist before
// this template.
@description('Location for the storage account.')
param location string

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'financeonefuncstorage'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

output id string = storage.id
output name string = storage.name
