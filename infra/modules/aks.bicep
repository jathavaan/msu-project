// Adopts the existing `financeone-aks` cluster, matching its current live config (single
// `nodepool1` system pool, Azure CNI Overlay, RBAC + Workload Identity + OIDC issuer already
// enabled) so this deployment is a no-op the first time it runs, and adds the Container Insights
// addon (issue #54) wired to the new Log Analytics workspace.
@description('Location for the cluster. Must match the resource group location used elsewhere.')
param location string

@description('SSH public key for the Linux node pool admin user. Leave the parameter file value as-is to keep the cluster\'s current key; pass a different one to rotate it.')
@secure()
param sshPublicKey string

@description('Resource ID of the Log Analytics workspace the Container Insights addon sends data to.')
param logAnalyticsWorkspaceId string

@description('Admin username for the Linux node pool.')
param adminUsername string = 'azureuser'

resource aks 'Microsoft.ContainerService/managedClusters@2024-05-01' = {
  name: 'financeone-aks'
  location: location
  sku: {
    name: 'Base'
    tier: 'Free'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    kubernetesVersion: '1.35'
    dnsPrefix: 'financeone-rg-financeone-ms-26743e'
    enableRBAC: true
    disableLocalAccounts: false
    supportPlan: 'KubernetesOfficial'
    agentPoolProfiles: [
      {
        name: 'nodepool1'
        mode: 'System'
        count: 1
        vmSize: 'Standard_B2s'
        osType: 'Linux'
        osSKU: 'Ubuntu'
        osDiskSizeGB: 128
        osDiskType: 'Managed'
        kubeletDiskType: 'OS'
        maxPods: 250
        enableAutoScaling: false
        type: 'VirtualMachineScaleSets'
        upgradeSettings: {
          maxSurge: '10%'
        }
      }
    ]
    linuxProfile: {
      adminUsername: adminUsername
      ssh: {
        publicKeys: [
          {
            keyData: sshPublicKey
          }
        ]
      }
    }
    networkProfile: {
      networkPlugin: 'azure'
      networkPluginMode: 'overlay'
      networkPolicy: 'none'
      networkDataplane: 'azure'
      loadBalancerSku: 'standard'
      outboundType: 'loadBalancer'
      podCidr: '10.244.0.0/16'
      serviceCidr: '10.0.0.0/16'
      dnsServiceIP: '10.0.0.10'
    }
    autoUpgradeProfile: {
      nodeOSUpgradeChannel: 'NodeImage'
    }
    oidcIssuerProfile: {
      enabled: true
    }
    securityProfile: {
      workloadIdentity: {
        enabled: true
      }
    }
    storageProfile: {
      diskCSIDriver: {
        enabled: true
      }
      fileCSIDriver: {
        enabled: true
      }
      snapshotController: {
        enabled: true
      }
    }
    workloadAutoScalerProfile: {
      keda: {
        enabled: false
      }
    }
    addonProfiles: {
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: logAnalyticsWorkspaceId
        }
      }
    }
  }
}

output id string = aks.id
output name string = aks.name
output oidcIssuerUrl string = aks.properties.oidcIssuerProfile.issuerURL
output kubeletIdentityObjectId string = aks.properties.identityProfile.kubeletidentity.objectId
