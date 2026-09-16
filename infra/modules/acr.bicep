// Adopts the existing `financeoneacr` registry, matching its current live config so this
// deployment is a no-op the first time it runs.
//
// NOTE: there is a second, unused registry in the resource group named `financeone` (login server
// financeone.azurecr.io) that predates this one and still holds a stray AcrPull grant for the AKS
// kubelet identity. It is not referenced by any k8s manifest or workflow and is deliberately left
// out of this template — see the PR description for cleanup options.
@description('Location for the registry. Must match the resource group location used elsewhere.')
param location string

resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: 'financeoneacr'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
    anonymousPullEnabled: false
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
    policies: {
      quarantinePolicy: {
        status: 'disabled'
      }
      trustPolicy: {
        type: 'Notary'
        status: 'disabled'
      }
      retentionPolicy: {
        days: 7
        status: 'disabled'
      }
      exportPolicy: {
        status: 'enabled'
      }
      azureADAuthenticationAsArmPolicy: {
        status: 'enabled'
      }
      softDeletePolicy: {
        retentionDays: 7
        status: 'disabled'
      }
    }
  }
}

output id string = acr.id
output name string = acr.name
output loginServer string = acr.properties.loginServer
