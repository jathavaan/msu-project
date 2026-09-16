// Adopts the existing `financeone-uami` managed identity. It is dual-purpose:
//  - CI/CD: GitHub Actions federates to it via OIDC (see federatedIdentityCredentials below) and
//    uses it to push images to ACR and to fetch AKS credentials.
//  - Runtime: AKS Workload Identity federates the `financeone-server` pod service account to it so
//    the pod can read secrets from Key Vault and authenticate to MySQL as an AAD user.
@description('Location for the identity. Must match the resource group location used elsewhere.')
param location string

@description('Name of the ACR (in this resource group) the identity needs AcrPush on.')
param acrName string

@description('Name of the AKS cluster (in this resource group) the identity needs "Azure Kubernetes Service Cluster User Role" on (used by workflow jobs calling az aks get-credentials without --admin).')
param aksName string

@description('Name of the Key Vault (in this resource group) the identity needs Key Vault Secrets User on.')
param keyVaultName string

@description('OIDC issuer URL of the AKS cluster, used to federate the workload identity credential.')
param aksOidcIssuerUrl string

@description('Kubernetes namespace the financeone-server service account lives in.')
param aksNamespace string = 'default'

@description('GitHub OIDC subject for the main-branch deploy workflow, exactly as currently trusted. Do not change without also updating the federated credential in Entra, or CI will start failing OIDC login.')
param githubMainBranchSubject string

@description('GitHub OIDC subject for the pull_request-triggered infra what-if job. Follows the same owner-id/repo-id subject format as githubMainBranchSubject; verify against Entra if PR OIDC login fails (see infra/README notes in the PR description).')
param githubPullRequestSubject string

resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: 'financeone-uami'
  location: location
}

resource federatedMainBranch 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2024-11-30' = {
  parent: uami
  name: 'github-actions-main'
  properties: {
    issuer: 'https://token.actions.githubusercontent.com'
    subject: githubMainBranchSubject
    audiences: [ 'api://AzureADTokenExchange' ]
  }
}

resource federatedPullRequest 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2024-11-30' = {
  parent: uami
  name: 'github-actions-pull-request'
  properties: {
    issuer: 'https://token.actions.githubusercontent.com'
    subject: githubPullRequestSubject
    audiences: [ 'api://AzureADTokenExchange' ]
  }
}

resource federatedWorkloadIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2024-11-30' = {
  parent: uami
  name: 'financeone-server-workload-identity'
  properties: {
    issuer: aksOidcIssuerUrl
    subject: 'system:serviceaccount:${aksNamespace}:financeone-server'
    audiences: [ 'api://AzureADTokenExchange' ]
  }
}

var acrPushRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '8311e382-0749-4cb8-b61a-304f252e45ec')
var aksClusterUserRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4abbcc35-e782-43d8-92c5-2d3f1bd2253f')
var keyVaultSecretsUserRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')

resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' existing = {
  name: acrName
}

resource aks 'Microsoft.ContainerService/managedClusters@2024-05-01' existing = {
  name: aksName
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Existing role assignment GUIDs, kept as explicit `name`s so this deployment updates the
// assignments already granted (via the Azure Portal) in place instead of erroring with
// "RoleAssignmentExists" trying to create a second one at the same scope for the same principal.
resource acrPush 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'b70572a8-8ffc-4441-9add-2af46cd9277a'
  scope: acr
  properties: {
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: acrPushRoleId
  }
}

resource aksClusterUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'ff28d849-bbaf-4992-b0b2-6a5be4f51316'
  scope: aks
  properties: {
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: aksClusterUserRoleId
  }
}

resource keyVaultSecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: '99f05dd1-7b6a-4a8a-b754-2f7d36e17b50'
  scope: keyVault
  properties: {
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: keyVaultSecretsUserRoleId
  }
}

var acrPullRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

// AKS creates its own kubelet identity (financeone-aks-agentpool) implicitly; it isn't a resource
// this template declares, but its AcrPull grant on the ACR the cluster actually pulls from is part
// of the live config and is adopted here so it isn't silently dropped from source control.
resource kubeletAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'ab948363-4733-498a-b60e-09789d82fb66'
  scope: acr
  properties: {
    principalId: aks.properties.identityProfile.kubeletidentity.objectId
    principalType: 'ServicePrincipal'
    roleDefinitionId: acrPullRoleId
  }
}

output principalId string = uami.properties.principalId
output clientId string = uami.properties.clientId
output id string = uami.id
