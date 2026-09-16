// Adopts the existing `financeone-key-vault`, matching its current live config so this deployment
// is a no-op the first time it runs. Access is RBAC-only (no access policies) — grants live in
// identity.bicep.
@description('Location for the vault. Must match the resource group location used elsewhere.')
param location string

@description('Entra tenant ID the vault belongs to.')
param tenantId string

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'financeone-key-vault'
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enablePurgeProtection: true
    publicNetworkAccess: 'Enabled'
    accessPolicies: []
  }
}

output id string = keyVault.id
output name string = keyVault.name
output vaultUri string = keyVault.properties.vaultUri
