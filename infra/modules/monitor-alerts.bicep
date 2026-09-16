// New Action Group + 4 metric alert rules — issue #54, did not exist before this template. The
// specific 4 alerts aren't specified by the issue; these cover the failure modes the deploy
// workflow's own rollout gate can't see (things that degrade *after* a rollout succeeds).
@description('Email address that receives alert notifications.')
param alertEmail string

@description('Resource ID of the AKS cluster to alert on.')
param aksResourceId string

@description('Resource ID of the MySQL flexible server to alert on.')
param mysqlResourceId string

@description('Resource ID of the Application Insights resource to alert on.')
param appInsightsResourceId string

resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: 'financeone-alerts'
  location: 'global'
  properties: {
    groupShortName: 'fin1alerts'
    enabled: true
    emailReceivers: [
      {
        name: 'owner'
        emailAddress: alertEmail
        useCommonAlertSchema: true
      }
    ]
  }
}

resource aksNodeCpuHigh 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: 'financeone-aks-node-cpu-high'
  location: 'global'
  properties: {
    description: 'AKS node CPU usage has stayed above 85% for 15 minutes.'
    severity: 2
    enabled: true
    scopes: [ aksResourceId ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    targetResourceType: 'Microsoft.ContainerService/managedClusters'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'NodeCpuHigh'
          metricName: 'node_cpu_usage_percentage'
          metricNamespace: 'Microsoft.ContainerService/managedClusters'
          operator: 'GreaterThan'
          threshold: 85
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

resource aksNodeMemoryHigh 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: 'financeone-aks-node-memory-high'
  location: 'global'
  properties: {
    description: 'AKS node working-set memory usage has stayed above 85% for 15 minutes.'
    severity: 2
    enabled: true
    scopes: [ aksResourceId ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    targetResourceType: 'Microsoft.ContainerService/managedClusters'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'NodeMemoryHigh'
          metricName: 'node_memory_working_set_percentage'
          metricNamespace: 'Microsoft.ContainerService/managedClusters'
          operator: 'GreaterThan'
          threshold: 85
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

resource mysqlStorageHigh 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: 'financeone-mysql-storage-high'
  location: 'global'
  properties: {
    description: 'MySQL Flexible Server storage usage has stayed above 85% for 15 minutes. storage.autoGrow is on, but this still gives advance warning before a resize.'
    severity: 2
    enabled: true
    scopes: [ mysqlResourceId ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    targetResourceType: 'Microsoft.DBforMySQL/flexibleServers'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'StoragePercentHigh'
          metricName: 'storage_percent'
          metricNamespace: 'Microsoft.DBforMySQL/flexibleServers'
          operator: 'GreaterThan'
          threshold: 85
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

resource apiServerExceptionsHigh 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: 'financeone-api-server-exceptions-high'
  location: 'global'
  properties: {
    description: 'FinanceOne.Api has logged more than 10 server exceptions in 15 minutes.'
    severity: 1
    enabled: true
    scopes: [ appInsightsResourceId ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    targetResourceType: 'Microsoft.Insights/components'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'ExceptionsHigh'
          metricName: 'exceptions/server'
          metricNamespace: 'microsoft.insights/components'
          operator: 'GreaterThan'
          threshold: 10
          // exceptions/server only supports Count as its time aggregation ("Time aggregation must
          // be one of [Count]") — Total, which every other alert in this file uses, isn't valid
          // for this specific metric.
          timeAggregation: 'Count'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

output actionGroupId string = actionGroup.id
