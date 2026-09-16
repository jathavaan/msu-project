// New Azure Communication Services resource + an Azure-managed Email domain, linked together, for
// the Function App to send coupon reminder / budget alert emails — issue #54, did not exist before
// this template.
//
// ACS and Email Communication Services are not regional resources in the usual sense; they take a
// `dataLocation` (a data-residency region grouping, not an Azure region) instead of `location`.
@description('ACS data residency grouping. "Europe" keeps data in the EU regardless of the resource group\'s actual Azure region.')
param dataLocation string = 'Europe'

resource emailService 'Microsoft.Communication/emailServices@2023-04-01' = {
  name: 'financeone-email'
  location: 'global'
  properties: {
    dataLocation: dataLocation
  }
}

resource emailDomain 'Microsoft.Communication/emailServices/domains@2023-04-01' = {
  parent: emailService
  name: 'AzureManagedDomain'
  location: 'global'
  properties: {
    domainManagement: 'AzureManaged'
    userEngagementTracking: 'Disabled'
  }
}

resource acs 'Microsoft.Communication/communicationServices@2023-04-01' = {
  name: 'financeone-acs'
  location: 'global'
  properties: {
    dataLocation: dataLocation
    linkedDomains: [
      emailDomain.id
    ]
  }
}

output acsId string = acs.id
output acsName string = acs.name
output emailDomainId string = emailDomain.id
// AzureManaged domains get a generated @<guid>.azurecomm.net sender address, verifiable only after
// deployment: `az communication email domain show`.
output fromSenderDomain string = emailDomain.properties.mailFromSenderDomain
