// Every RBAC grant financeone-uami (and the AKS kubelet identity) needs, kept OUT of main.bicep on
// purpose: this subscription has an ABAC condition on role-assignment delegation that allows
// granting "Contributor" but blocks "User Access Administrator" (confirmed by trying it —
// AuthorizationFailed with an ABAC condition reason). financeone-uami has Contributor (enough for
// every other resource in main.bicep) but can never itself hold roleAssignments/write, so it can
// never run this file. That means someone with sufficient rights runs this by hand, same as the
// Entra app registration in the multi-user issue is a documented manual step rather than fought
// through IaC.
//
// Deploy once, and again whenever a role is added/changed here:
//   az deployment group create -g rg-financeone-msu -f infra/role-assignments.bicep -p infra/role-assignments.parameters.json
//
// The first four grants already exist (created by hand before this template) — this adopts them by
// name so re-running is a no-op. The last four are new, for the Storage/Functions modules in
// main.bicep, and must be applied before the Function App can actually reach its storage account.
@description('Principal ID of financeone-uami.')
param uamiPrincipalId string

@description('Name of financeoneacr.')
param acrName string

@description('Name of financeone-aks.')
param aksName string

@description('Name of financeone-key-vault.')
param keyVaultName string

@description('Name of the app storage account (storage-app.bicep).')
param appStorageAccountName string

@description('Name of the Functions storage account (storage-functions.bicep).')
param functionsStorageAccountName string

resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' existing = {
  name: acrName
}

resource aks 'Microsoft.ContainerService/managedClusters@2024-05-01' existing = {
  name: aksName
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource appStorage 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: appStorageAccountName
}

resource functionsStorage 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: functionsStorageAccountName
}

var acrPushRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '8311e382-0749-4cb8-b61a-304f252e45ec')
var acrPullRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
var aksClusterUserRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4abbcc35-e782-43d8-92c5-2d3f1bd2253f')
var keyVaultSecretsUserRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
var storageBlobDataContributorRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
var storageBlobDataOwnerRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')
var storageQueueDataContributorRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '974c5e8b-45b9-4653-ba55-5f855dd0fb88')
var storageTableDataContributorRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3')

// --- Pre-existing grants (adopted by their real GUID names so this is a no-op update) ---

resource acrPush 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'b70572a8-8ffc-4441-9add-2af46cd9277a'
  scope: acr
  properties: {
    principalId: uamiPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: acrPushRoleId
  }
}

resource aksClusterUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'ff28d849-bbaf-4992-b0b2-6a5be4f51316'
  scope: aks
  properties: {
    principalId: uamiPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: aksClusterUserRoleId
  }
}

resource keyVaultSecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: '99f05dd1-7b6a-4a8a-b754-2f7d36e17b50'
  scope: keyVault
  properties: {
    principalId: uamiPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: keyVaultSecretsUserRoleId
  }
}

// AKS creates its own kubelet identity (financeone-aks-agentpool) implicitly; it isn't declared in
// main.bicep, but its AcrPull grant on the ACR the cluster actually pulls from is part of the live
// config and is adopted here so it isn't silently dropped from source control.
resource kubeletAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'ab948363-4733-498a-b60e-09789d82fb66'
  scope: acr
  properties: {
    principalId: aks.properties.identityProfile.kubeletidentity.objectId
    principalType: 'ServicePrincipal'
    roleDefinitionId: acrPullRoleId
  }
}

// --- New grants, needed once storage-app.bicep / functions.bicep have been deployed ---

resource blobDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(appStorage.id, uamiPrincipalId, storageBlobDataContributorRoleId)
  scope: appStorage
  properties: {
    principalId: uamiPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageBlobDataContributorRoleId
  }
}

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
