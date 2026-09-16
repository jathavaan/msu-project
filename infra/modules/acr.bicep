// Adopts the existing `financeoneacr` registry, matching its current live config so this
// deployment is a no-op the first time it runs.
//
// A second registry named `financeone` also exists in the resource group — a leftover from before
// the client/server split, not referenced by any k8s manifest or workflow, still holding a stray
// AcrPull grant for the AKS kubelet identity. It was never declared in this template on purpose;
// removing it (registry + role assignment, both live-only, nothing here to update) is tracked in
// issue #67.
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
