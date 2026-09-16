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

// A Microsoft.OperationsManagement/solutions "ContainerInsights(...)" resource would link this
// workspace into the Azure Portal's dedicated AKS Insights dashboards. Deliberately left out: it
// needs the Microsoft.OperationsManagement resource provider registered on the subscription, which
// this one isn't, purely for a cosmetic portal integration — the omsagent addon in aks.bicep still
// sends data to this workspace either way, just browsable as a raw workspace instead of through the
// Insights blade. Add it back if the provider ever gets registered and the nicer dashboards matter.
output id string = workspace.id
output name string = workspace.name
output customerId string = workspace.properties.customerId
