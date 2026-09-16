// Adopts the existing `financeone-uami` managed identity. It is dual-purpose:
//  - CI/CD: GitHub Actions federates to it via OIDC (see federatedIdentityCredentials below) and
//    uses it to push images to ACR and to fetch AKS credentials.
//  - Runtime: AKS Workload Identity federates the `financeone-server` pod service account to it so
//    the pod can read secrets from Key Vault and authenticate to MySQL as an AAD user.
//
// The role assignments that grant it AcrPush/AKS Cluster User/Key Vault Secrets User etc. are
// deliberately NOT declared here — see ../role-assignments.bicep for why.
@description('Location for the identity. Must match the resource group location used elsewhere.')
param location string

@description('OIDC issuer URL of the AKS cluster, used to federate the workload identity credential.')
param aksOidcIssuerUrl string

@description('Kubernetes namespace the financeone-server service account lives in.')
param aksNamespace string = 'default'

@description('GitHub OIDC subject for the main-branch deploy workflow, exactly as currently trusted. Do not change without also updating the federated credential in Entra, or CI will start failing OIDC login.')
param githubMainBranchSubject string

@description('GitHub OIDC subject for the pull_request-triggered infra what-if job. Follows the same owner-id/repo-id subject format as githubMainBranchSubject; verify against Entra if PR OIDC login fails.')
param githubPullRequestSubject string

@description('GitHub OIDC subject for the deploy job. NOT the same shape as githubMainBranchSubject — a job that sets `environment:` gets a subject of the form repo:OWNER/REPO:environment:NAME instead of the usual ref:refs/heads/BRANCH one, because the environment protection rule (not the branch) is what GitHub is asserting. Learned this the hard way when the first deploy run failed AADSTS700213.')
param githubEnvironmentSubject string

resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: 'financeone-uami'
  location: location
}

resource federatedMainBranch 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2024-11-30' = {
  parent: uami
  name: 'github-actions-main'
  properties: {
    issuer: 'https://token.actions.githubusercontent.com'
    subject: githubMainBranchSubject
    audiences: [ 'api://AzureADTokenExchange' ]
  }
}

resource federatedPullRequest 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2024-11-30' = {
  parent: uami
  name: 'github-actions-pull-request'
  properties: {
    issuer: 'https://token.actions.githubusercontent.com'
    subject: githubPullRequestSubject
    audiences: [ 'api://AzureADTokenExchange' ]
  }
}

resource federatedEnvironment 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2024-11-30' = {
  parent: uami
  name: 'github-actions-infra-production'
  properties: {
    issuer: 'https://token.actions.githubusercontent.com'
    subject: githubEnvironmentSubject
    audiences: [ 'api://AzureADTokenExchange' ]
  }
}

resource federatedWorkloadIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2024-11-30' = {
  parent: uami
  name: 'financeone-server-workload-identity'
  properties: {
    issuer: aksOidcIssuerUrl
    subject: 'system:serviceaccount:${aksNamespace}:financeone-server'
    audiences: [ 'api://AzureADTokenExchange' ]
  }
}

output principalId string = uami.properties.principalId
output clientId string = uami.properties.clientId
output id string = uami.id
