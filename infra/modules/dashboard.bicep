// Azure Portal dashboard (issue #51) surfacing the metrics behind the alerts in
// monitor-alerts.bicep: API health from Application Insights, AKS node pressure, and MySQL
// pressure. Uses Microsoft.Portal/dashboards, which has a loosely-typed `properties` blob (the
// portal's own dashboard JSON) rather than a strict ARM schema — Bicep can't validate the part
// layout beyond basic JSON shape.
//
// Pod crash/restart-loop visibility (also asked for in issue #51) isn't included: it isn't an AKS
// platform metric under the Container Insights (Log Analytics) integration this repo has today —
// only the underlying Managed Prometheus add-on exposes a `kube_pod_status_*` metric, and aks.bicep
// doesn't enable it. Same gap as the alert rules in monitor-alerts.bicep, which also stops at 4 of
// the 4 failure modes the issue lists minus this one. Would need either a Log Analytics query tile
// against KubePodInventory/Perf, or turning on Managed Prometheus — both follow-up work.
//
// This dashboard part schema has no supported way to draw a literal threshold line on a metrics
// chart, so the four metrics that already have a matching alert in monitor-alerts.bicep get a
// plain-text caption tile stating the threshold and the alert rule name instead.
//
// Chart tiles use `Extension/HubsExtension/PartType/MonitorChartPart` — the schema the Metrics
// blade's own "Pin to dashboard" currently generates (verified against Microsoft's own
// applicationinsights-dashboard.bicep sample) — not the `Extension/Microsoft_Azure_Monitoring/
// PartType/MetricsChartPart` shown in Microsoft's "structure of Azure dashboards" doc: that one is
// still valid JSON and deploys without error, but renders "We've unfortunately encountered an
// error for this chart" for every tile in the current portal. aggregationType is numeric here:
// 4 = Average, 7 = Count (confirmed against the same sample — Average for gauge/percentage
// metrics, Count for occurrence counts like exceptions/failures).
@description('Location for the dashboard resource.')
param location string

@description('Resource ID of the Application Insights resource to chart.')
param appInsightsResourceId string

@description('Resource ID of the AKS cluster to chart.')
param aksResourceId string

@description('Resource ID of the MySQL flexible server to chart.')
param mysqlResourceId string

func monitorChartPart(position object, resourceId string, namespace string, metricName string, displayName string, aggregationType int) object => {
  position: position
  metadata: {
    inputs: [
      {
        name: 'options'
        value: {
          chart: {
            metrics: [
              {
                resourceMetadata: {
                  id: resourceId
                }
                name: metricName
                aggregationType: aggregationType
                namespace: namespace
                metricVisualization: {
                  displayName: displayName
                }
              }
            ]
            title: displayName
            visualization: {
              chartType: 2
              legendVisualization: {
                isVisible: true
                position: 2
                hideSubtitle: false
              }
              axisVisualization: {
                x: {
                  isVisible: true
                  axisType: 2
                }
                y: {
                  isVisible: true
                  axisType: 1
                }
              }
            }
          }
        }
      }
      {
        name: 'sharedTimeRange'
        isOptional: true
      }
    ]
    type: 'Extension/HubsExtension/PartType/MonitorChartPart'
    settings: {}
  }
}

func markdownPart(position object, content string) object => {
  position: position
  metadata: {
    inputs: []
    type: 'Extension/HubsExtension/PartType/MarkdownPart'
    settings: {
      content: {
        settings: {
          content: content
          title: ''
          subtitle: ''
          markdownSource: 1
        }
      }
    }
  }
}

var appInsightsNamespace = 'microsoft.insights/components'
var aksNamespace = 'Microsoft.ContainerService/managedClusters'
var mysqlNamespace = 'Microsoft.DBforMySQL/flexibleServers'

var parts = [
  markdownPart({ x: 0, y: 0, colSpan: 12, rowSpan: 1 }, '# FinanceOne Overview\nAPI health, AKS node pressure, and MySQL pressure — the same signals monitor-alerts.bicep pages on.')

  markdownPart({ x: 0, y: 1, colSpan: 12, rowSpan: 1 }, '## API / Application Insights')
  monitorChartPart({ x: 0, y: 2, colSpan: 6, rowSpan: 4 }, appInsightsResourceId, appInsightsNamespace, 'requests/count', 'Server requests', 7)
  monitorChartPart({ x: 6, y: 2, colSpan: 6, rowSpan: 4 }, appInsightsResourceId, appInsightsNamespace, 'requests/duration', 'Server response time', 4)
  markdownPart({ x: 6, y: 6, colSpan: 6, rowSpan: 1 }, 'Alert threshold: more than 10 exceptions in 15 minutes (`financeone-api-server-exceptions-high`)')
  monitorChartPart({ x: 0, y: 7, colSpan: 6, rowSpan: 4 }, appInsightsResourceId, appInsightsNamespace, 'requests/failed', 'Failed requests', 7)
  monitorChartPart({ x: 6, y: 7, colSpan: 6, rowSpan: 4 }, appInsightsResourceId, appInsightsNamespace, 'exceptions/server', 'Server exceptions', 7)

  markdownPart({ x: 0, y: 11, colSpan: 12, rowSpan: 1 }, '## AKS / Container Insights')
  markdownPart({ x: 0, y: 12, colSpan: 6, rowSpan: 1 }, 'Alert threshold: average above 85% for 15 minutes (`financeone-aks-node-cpu-high`)')
  markdownPart({ x: 6, y: 12, colSpan: 6, rowSpan: 1 }, 'Alert threshold: average above 85% for 15 minutes (`financeone-aks-node-memory-high`)')
  monitorChartPart({ x: 0, y: 13, colSpan: 6, rowSpan: 4 }, aksResourceId, aksNamespace, 'node_cpu_usage_percentage', 'AKS node CPU %', 4)
  monitorChartPart({ x: 6, y: 13, colSpan: 6, rowSpan: 4 }, aksResourceId, aksNamespace, 'node_memory_working_set_percentage', 'AKS node memory %', 4)

  markdownPart({ x: 0, y: 17, colSpan: 12, rowSpan: 1 }, '## MySQL')
  monitorChartPart({ x: 0, y: 18, colSpan: 6, rowSpan: 4 }, mysqlResourceId, mysqlNamespace, 'cpu_percent', 'MySQL CPU %', 4)
  monitorChartPart({ x: 6, y: 18, colSpan: 6, rowSpan: 4 }, mysqlResourceId, mysqlNamespace, 'active_connections', 'MySQL active connections', 4)
  markdownPart({ x: 0, y: 22, colSpan: 6, rowSpan: 1 }, 'Alert threshold: average above 85% for 15 minutes (`financeone-mysql-storage-high`)')
  monitorChartPart({ x: 0, y: 23, colSpan: 6, rowSpan: 4 }, mysqlResourceId, mysqlNamespace, 'storage_percent', 'MySQL storage %', 4)
  monitorChartPart({ x: 6, y: 23, colSpan: 6, rowSpan: 4 }, mysqlResourceId, mysqlNamespace, 'io_consumption_percent', 'MySQL IO consumption %', 4)
]

resource dashboard 'Microsoft.Portal/dashboards@2015-08-01-preview' = {
  name: 'financeone-dashboard'
  location: location
  tags: {
    'hidden-title': 'FinanceOne Overview'
  }
  properties: {
    // Microsoft.Portal/dashboards is the classic pre-Bicep dashboard format: both `lenses` and a
    // lens's `parts` are JSON objects keyed by stringified index ("0", "1", ...), not arrays, even
    // though Bicep's own type for them looks array-shaped. An array literal here passes `bicep
    // build` (just a BCP036 warning) but fails ARM preflight validation at apply time, so `parts`
    // is built as an object via toObject rather than written as a `parts: [...]` literal.
    lenses: {
      '0': {
        order: 0
        parts: toObject(range(0, length(parts)), i => string(i), i => parts[i])
      }
    }
    metadata: {
      model: {
        timeRange: {
          value: {
            relative: {
              duration: 24
              timeUnit: 'hours'
            }
          }
          type: 'MsPortalFx.Composition.Configuration.ValueTypes.TimeRange'
        }
      }
    }
  }
}

output id string = dashboard.id
output name string = dashboard.name
