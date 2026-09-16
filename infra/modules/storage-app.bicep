// New storage account for application blob data (coupons/, exports/, staged-csv/) — issue #54.
// Did not exist before this template; nothing in the app currently reads/writes it yet.
//
// financeone-uami's Storage Blob Data Contributor grant on this account lives in
// ../role-assignments.bicep, not here — see that file for why RBAC grants are kept out of the
// CI-deployed template.
@description('Location for the storage account.')
param location string

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'financeoneappstorage'
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

  resource blobServices 'blobServices' = {
    name: 'default'

    resource coupons 'containers' = {
      name: 'coupons'
      properties: {
        publicAccess: 'None'
      }
    }

    resource exports 'containers' = {
      name: 'exports'
      properties: {
        publicAccess: 'None'
      }
    }

    resource stagedCsv 'containers' = {
      name: 'staged-csv'
      properties: {
        publicAccess: 'None'
      }
    }
  }
}

output id string = storage.id
output name string = storage.name
