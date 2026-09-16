// Composes every module for the single `rg-financeone-msu` environment. Deploy with:
//   az deployment group create -g rg-financeone-msu -f infra/main.bicep -p infra/main.parameters.json
// Preview first with `what-if` in place of `create` — see .github/workflows/infra.yaml, which runs
// that automatically on every PR touching infra/**.
targetScope = 'resourceGroup'

@description('Azure region for regional resources. All adopted resources already live in swedencentral; changing this would not move them.')
param location string = 'swedencentral'

@description('Entra tenant ID.')
param tenantId string

@secure()
@description('SSH public key for the AKS Linux node pool.')
param sshPublicKey string

@description('Object ID (sid) of the AAD principal that administers financeone-sqlserver.')
param aadAdminObjectId string

@description('UPN/display login of the MySQL AAD administrator.')
param aadAdminLogin string

@secure()
@description('MySQL administrator password. Leave blank — see mysql.bicep for why.')
param administratorLoginPassword string = ''

@description('Kubernetes namespace the financeone-server workload runs in.')
param aksNamespace string = 'default'

@description('GitHub OIDC subject currently trusted for the main-branch deploy workflow.')
param githubMainBranchSubject string

@description('GitHub OIDC subject for the pull_request-triggered infra what-if job.')
param githubPullRequestSubject string

@description('Email address that receives Azure Monitor alert notifications.')
param alertEmail string

module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'log-analytics'
  params: {
    location: location
  }
}

module appInsights 'modules/app-insights.bicep' = {
  name: 'app-insights'
  params: {
    location: location
    logAnalyticsWorkspaceId: logAnalytics.outputs.id
  }
}

module acr 'modules/acr.bicep' = {
  name: 'acr'
  params: {
    location: location
  }
}

module aks 'modules/aks.bicep' = {
  name: 'aks'
  params: {
    location: location
    sshPublicKey: sshPublicKey
    logAnalyticsWorkspaceId: logAnalytics.outputs.id
  }
}

module keyVault 'modules/keyvault.bicep' = {
  name: 'keyvault'
  params: {
    location: location
    tenantId: tenantId
  }
}

module mysql 'modules/mysql.bicep' = {
  name: 'mysql'
  params: {
    location: location
    aadAdminIdentityResourceId: identity.outputs.id
    aadAdminObjectId: aadAdminObjectId
    aadAdminLogin: aadAdminLogin
    tenantId: tenantId
    administratorLoginPassword: administratorLoginPassword
  }
}

module identity 'modules/identity.bicep' = {
  name: 'identity'
  params: {
    location: location
    acrName: acr.outputs.name
    aksName: aks.outputs.name
    keyVaultName: keyVault.outputs.name
    aksOidcIssuerUrl: aks.outputs.oidcIssuerUrl
    aksNamespace: aksNamespace
    githubMainBranchSubject: githubMainBranchSubject
    githubPullRequestSubject: githubPullRequestSubject
  }
}

module storageApp 'modules/storage-app.bicep' = {
  name: 'storage-app'
  params: {
    location: location
    blobDataContributorPrincipalId: identity.outputs.principalId
  }
}

module storageFunctions 'modules/storage-functions.bicep' = {
  name: 'storage-functions'
  params: {
    location: location
  }
}

module communication 'modules/communication.bicep' = {
  name: 'communication'
}

module functions 'modules/functions.bicep' = {
  name: 'functions'
  params: {
    location: location
    functionsStorageAccountName: storageFunctions.outputs.name
    appInsightsConnectionString: appInsights.outputs.connectionString
    uamiResourceId: identity.outputs.id
    uamiClientId: identity.outputs.clientId
    uamiPrincipalId: identity.outputs.principalId
    keyVaultUri: keyVault.outputs.vaultUri
  }
}

module monitorAlerts 'modules/monitor-alerts.bicep' = {
  name: 'monitor-alerts'
  params: {
    alertEmail: alertEmail
    aksResourceId: aks.outputs.id
    mysqlResourceId: mysql.outputs.id
    appInsightsResourceId: appInsights.outputs.id
  }
}

output acrLoginServer string = acr.outputs.loginServer
output aksName string = aks.outputs.name
output keyVaultUri string = keyVault.outputs.vaultUri
output mysqlFqdn string = mysql.outputs.fullyQualifiedDomainName
output functionAppName string = functions.outputs.name
output acsName string = communication.outputs.acsName
