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

// Links the workspace into the Azure Portal's dedicated "Insights" blade for the AKS cluster.
// Needs the Microsoft.OperationsManagement resource provider registered on the subscription — it
// turns out that's true regardless of whether this resource is declared, since the omsagent addon
// in aks.bicep needs it too to actually enable Container Insights. Registered via
// `az provider register --namespace Microsoft.OperationsManagement` (safe/reversible, no cost —
// see the PR description), so there's no reason to skip the nicer dashboard on top of that.
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
