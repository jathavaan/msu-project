// New storage account for application blob data (coupons/, exports/, staged-csv/) — issue #54.
// Did not exist before this template; nothing in the app currently reads/writes it yet.
@description('Location for the storage account.')
param location string

@description('Principal ID granted Storage Blob Data Contributor on this account (financeone-uami, shared by the API pod and the Function App).')
param blobDataContributorPrincipalId string

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

var storageBlobDataContributorRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')

resource blobDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, blobDataContributorPrincipalId, storageBlobDataContributorRoleId)
  scope: storage
  properties: {
    principalId: blobDataContributorPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageBlobDataContributorRoleId
  }
}

output id string = storage.id
output name string = storage.name
