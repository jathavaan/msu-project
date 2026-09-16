// Adopts the existing `financeone-sqlserver` MySQL Flexible Server, matching its current live
// config so this deployment is a no-op the first time it runs.
//
// aad_auth_only is ON — there is no MySQL-native password auth on this server at all. The
// `financeone-uami` AAD database user (created via `CREATE AADUSER` against the AAD login/object
// id already granted the "AAD admin" role below) is a data-plane statement, not an ARM resource,
// and is *not* managed by this template — see the PR description for the one-time manual step.
@description('Location for the server. Must match the resource group location used elsewhere.')
param location string

@description('Resource ID of the financeone-uami identity, used by MySQL to resolve the AAD administrator.')
param aadAdminIdentityResourceId string

@description('Object ID (sid) of the AAD principal that is the server\'s Active Directory administrator.')
param aadAdminObjectId string

@description('UPN/display login of the AAD administrator principal.')
param aadAdminLogin string

@description('Entra tenant ID the server and its AAD administrator belong to.')
param tenantId string

@secure()
@description('Administrator password. Only used the first time the server is created — aad_auth_only=ON means MySQL password auth is disabled afterwards, so this can be left blank on every subsequent deployment; the platform ignores it on updates to an existing server.')
param administratorLoginPassword string = ''

resource mysql 'Microsoft.DBforMySQL/flexibleServers@2024-06-01-preview' = {
  name: 'financeone-sqlserver'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${aadAdminIdentityResourceId}': {}
    }
  }
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '8.4'
    administratorLogin: 'financeone'
    administratorLoginPassword: empty(administratorLoginPassword) ? null : administratorLoginPassword
    availabilityZone: '3'
    // storageSku is a read-only/derived property on this resource (Azure sets it to Premium_LRS
    // itself based on size+tier) and cannot be assigned here.
    storage: {
      storageSizeGB: 20
      autoGrow: 'Enabled'
      autoIoScaling: 'Enabled'
      iops: 360
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

resource aadAuthOnly 'Microsoft.DBforMySQL/flexibleServers/configurations@2024-06-01-preview' = {
  parent: mysql
  name: 'aad_auth_only'
  properties: {
    value: 'ON'
    source: 'user-override'
  }
}

resource aadAdmin 'Microsoft.DBforMySQL/flexibleServers/administrators@2024-06-01-preview' = {
  parent: mysql
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    identityResourceId: aadAdminIdentityResourceId
    login: aadAdminLogin
    sid: aadAdminObjectId
    tenantId: tenantId
  }
  dependsOn: [
    aadAuthOnly
  ]
}

// Special "magic" rule (0.0.0.0-0.0.0.0) that lets other Azure resources (AKS pods, this
// deployment's own what-if/create runs) reach the server without listing individual IPs.
//
// A second firewall rule allowing one specific developer IP exists on the live server for ad-hoc
// `mysql` client access. It is deliberately left out of this template — adopting it would commit a
// personal IP address to source control, and dropping it here would delete that access on the next
// deploy. Recreate it out-of-band (`az mysql flexible-server firewall-rule create`) if you still
// need direct client access.
resource allowAzureServices 'Microsoft.DBforMySQL/flexibleServers/firewallRules@2024-06-01-preview' = {
  parent: mysql
  name: 'AllowAllAzureServicesAndResourcesWithinAzureIps_2026-9-2_14-59-36'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// `existing`, not managed: the ARM REST API refuses to change a database's charset/collation once
// created at all ("DatabaseCharsetOrCollationConflict" — the fix, per the error, is `ALTER DATABASE`
// over a SQL connection, not this API). Declaring this as a normal resource made every deployment
// fail on it regardless of what properties were set (or omitted), since ARM tries to reconcile the
// full resource either way. Kept here purely so financeone-db's existence is documented; nothing
// else in this template references it.
resource database 'Microsoft.DBforMySQL/flexibleServers/databases@2024-06-01-preview' existing = {
  parent: mysql
  name: 'financeone-db'
}

output id string = mysql.id
output name string = mysql.name
output fullyQualifiedDomainName string = mysql.properties.fullyQualifiedDomainName
output databaseName string = database.name
