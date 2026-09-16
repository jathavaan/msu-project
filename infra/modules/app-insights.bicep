// New workspace-based Application Insights resource for the API (issue #54) — did not exist
// before this template. Not yet wired into FinanceOne.Api; adding the connection string to
// appsettings/Key Vault and the SDK package is follow-up application work, not infra.
@description('Location for the Application Insights resource.')
param location string

@description('Resource ID of the Log Analytics workspace backing this workspace-based Application Insights resource.')
param logAnalyticsWorkspaceId string

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'financeone-app-insights'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspaceId
    IngestionMode: 'LogAnalytics'
  }
}

output id string = appInsights.id
output name string = appInsights.name
output connectionString string = appInsights.properties.ConnectionString
