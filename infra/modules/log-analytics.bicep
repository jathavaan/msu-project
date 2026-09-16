// New workspace shared by the AKS Container Insights addon and Application Insights (issue #54)
// — neither existed before this template.
@description('Location for the workspace.')
param location string

@description('Days of log retention. 30 is the minimum billable tier above the free 7-day default and is enough for a single-environment personal project.')
param retentionInDays int = 30

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'financeone-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: retentionInDays
  }
}

// Links the workspace into the Azure Portal's "Insights" blade for the AKS cluster. Purely
// cosmetic — the omsagent addon in aks.bicep sends data to the workspace regardless of whether
// this solution exists — but without it the portal shows the raw workspace instead of the AKS
// Insights dashboards.
resource containerInsightsSolution 'Microsoft.OperationsManagement/solutions@2015-11-01-preview' = {
  name: 'ContainerInsights(${workspace.name})'
  location: location
  plan: {
    name: 'ContainerInsights(${workspace.name})'
    product: 'OMSGallery/ContainerInsights'
    publisher: 'Microsoft'
    promotionCode: ''
  }
  properties: {
    workspaceResourceId: workspace.id
  }
}

output id string = workspace.id
output name string = workspace.name
output customerId string = workspace.properties.customerId
