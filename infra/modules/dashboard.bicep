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
@description('Location for the dashboard resource.')
param location string

@description('Resource ID of the Application Insights resource to chart.')
param appInsightsResourceId string

@description('Resource ID of the AKS cluster to chart.')
param aksResourceId string

@description('Resource ID of the MySQL flexible server to chart.')
param mysqlResourceId string

func metricsChartPart(position object, resourceId string, metricName string) object => {
  position: position
  metadata: {
    inputs: [
      {
        name: 'queryInputs'
        value: {
          timespan: {
            duration: 'PT1H'
          }
          id: resourceId
          chartType: 0
          metrics: [
            {
              name: metricName
              resourceId: resourceId
            }
          ]
        }
      }
    ]
    type: 'Extension/Microsoft_Azure_Monitoring/PartType/MetricsChartPart'
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

var parts = [
  markdownPart({ x: 0, y: 0, colSpan: 12, rowSpan: 1 }, '# FinanceOne Overview\nAPI health, AKS node pressure, and MySQL pressure — the same signals monitor-alerts.bicep pages on.')

  markdownPart({ x: 0, y: 1, colSpan: 12, rowSpan: 1 }, '## API / Application Insights')
  metricsChartPart({ x: 0, y: 2, colSpan: 6, rowSpan: 4 }, appInsightsResourceId, 'requests/count')
  metricsChartPart({ x: 6, y: 2, colSpan: 6, rowSpan: 4 }, appInsightsResourceId, 'requests/duration')
  markdownPart({ x: 6, y: 6, colSpan: 6, rowSpan: 1 }, 'Alert threshold: more than 10 exceptions in 15 minutes (`financeone-api-server-exceptions-high`)')
  metricsChartPart({ x: 0, y: 7, colSpan: 6, rowSpan: 4 }, appInsightsResourceId, 'requests/failed')
  metricsChartPart({ x: 6, y: 7, colSpan: 6, rowSpan: 4 }, appInsightsResourceId, 'exceptions/server')

  markdownPart({ x: 0, y: 11, colSpan: 12, rowSpan: 1 }, '## AKS / Container Insights')
  markdownPart({ x: 0, y: 12, colSpan: 6, rowSpan: 1 }, 'Alert threshold: average above 85% for 15 minutes (`financeone-aks-node-cpu-high`)')
  markdownPart({ x: 6, y: 12, colSpan: 6, rowSpan: 1 }, 'Alert threshold: average above 85% for 15 minutes (`financeone-aks-node-memory-high`)')
  metricsChartPart({ x: 0, y: 13, colSpan: 6, rowSpan: 4 }, aksResourceId, 'node_cpu_usage_percentage')
  metricsChartPart({ x: 6, y: 13, colSpan: 6, rowSpan: 4 }, aksResourceId, 'node_memory_working_set_percentage')

  markdownPart({ x: 0, y: 17, colSpan: 12, rowSpan: 1 }, '## MySQL')
  metricsChartPart({ x: 0, y: 18, colSpan: 6, rowSpan: 4 }, mysqlResourceId, 'cpu_percent')
  metricsChartPart({ x: 6, y: 18, colSpan: 6, rowSpan: 4 }, mysqlResourceId, 'active_connections')
  markdownPart({ x: 0, y: 22, colSpan: 6, rowSpan: 1 }, 'Alert threshold: average above 85% for 15 minutes (`financeone-mysql-storage-high`)')
  metricsChartPart({ x: 0, y: 23, colSpan: 6, rowSpan: 4 }, mysqlResourceId, 'storage_percent')
  metricsChartPart({ x: 6, y: 23, colSpan: 6, rowSpan: 4 }, mysqlResourceId, 'io_consumption_percent')
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
