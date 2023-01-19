param storageName string
param containerNames array = [ 'con','training-snapshot' ]
param location string = resourceGroup().location
param functionAppName string
param cvFunctionAppName string
param sqlServerName string
param sqlServerAdminLogin string
@secure()
param sqlServerAdminLoginPassword string
param emailCommServiceName string
param commServiceName string
param dbName string
param customvisionProjectName string
param customvisionModelName string
param computervisionName string
param customvisionName string
param dependenciesShareName string

resource appStorage 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: storageName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Cool'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'

  }
}

resource sqlServer 'Microsoft.Sql/servers@2022-02-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlServerAdminLogin
    administratorLoginPassword: sqlServerAdminLoginPassword
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-02-01-preview' = {
  name: dbName
  location: location
  sku: {
    capacity: 5
    name: 'Free'
    tier: 'Free'
  }
  parent: sqlServer
}

resource dbFirewallRules 'Microsoft.Sql/servers/firewallRules@2022-02-01-preview'= {
  parent: sqlServer
  name: 'AllowAllAzureIPs'
  properties:{
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

resource emailCommunicationService 'Microsoft.Communication/emailServices@2022-07-01-preview' = {
  name: emailCommServiceName
  location: 'global'
  properties: {
    dataLocation: 'United States'
  }

}
resource emailCommunicationDomain 'Microsoft.Communication/emailServices/domains@2022-07-01-preview' = {
  name: 'AzureManagedDomain'
  location: 'global'
  parent: emailCommunicationService
  properties: {
    domainManagement: 'AzureManaged'
  }
}

resource communicationService 'Microsoft.Communication/communicationServices@2022-07-01-preview' = {
  name: commServiceName
  location: 'global'
  properties: {
    dataLocation: 'United States'
    linkedDomains: [
      emailCommunicationDomain.id
    ]
  }
}

resource imageContainers 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = [for name in containerNames: {
  name: '${appStorage.name}/default/${name}'
  properties: {
    publicAccess: 'Blob'
  }
}]

resource queue 'Microsoft.Storage/storageAccounts/queueServices/queues@2021-09-01' = {
  name: '${appStorage.name}/default/altitude-position'
}
resource queue2 'Microsoft.Storage/storageAccounts/queueServices/queues@2021-09-01' = {
  name: '${appStorage.name}/default/ecam-position'
}

resource fileshare 'Microsoft.Storage/storageAccounts/fileServices/shares@2022-05-01' = {
  name: '${appStorage.name}/default/dependencies'
}

resource computervision 'Microsoft.CognitiveServices/accounts@2022-03-01' = {
  name: computervisionName
  location: location
  sku: {
    name: 'F0'
  }
  kind: 'ComputerVision'
  identity: {
    type: 'None'
  }
  properties: {
    customSubDomainName: computervisionName
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
    publicNetworkAccess: 'Enabled'
  }
}

resource customvisionTraining 'Microsoft.CognitiveServices/accounts@2022-03-01' = {
  name: customvisionName
  location: location
  sku: {
    name: 'F0'
  }
  kind: 'CustomVision.Training'
  properties: {
    customSubDomainName: customvisionName
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
    publicNetworkAccess: 'Enabled'
  }
}

resource customvisionPrediction 'Microsoft.CognitiveServices/accounts@2022-03-01' = {
  name: '${customvisionName}-prediction'
  location: location
  sku: {
    name: 'F0'
  }
  kind: 'CustomVision.Prediction'
  properties: {
    customSubDomainName: '${customvisionName}-prediction'
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
    publicNetworkAccess: 'Enabled'
  }
}

resource cvFunctionApp 'Microsoft.Web/sites@2022-03-01' = {
  name: cvFunctionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${appStorage.listKeys().keys[0].value}'
        }
        {
          name: 'DependenciesShareName'
          value: dependenciesShareName
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'python'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
      ]
      minTlsVersion: '1.2'
    }
  }
}

resource functionApp 'Microsoft.Web/sites@2022-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${appStorage.listKeys().keys[0].value}'
        }
        {
          name: 'connectionString'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${dbName};Persist Security Info=False;User ID=${sqlServerAdminLogin};Password=${sqlServerAdminLoginPassword};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
        {
          name: 'ConnectionEmail'
          value: 'endpoint=https://${commServiceName}.communication.azure.com/;accesskey=${communicationService.listKeys().primaryKey}'
        }
        {
          name: 'EmailSender'
          value: 'DoNotReply@${emailCommunicationDomain.properties.mailFromSenderDomain}'
        }
        {
          name: 'CustomVisionPredictionKey'
          value: customvisionPrediction.listKeys().key1
        }
        {
          name: 'CustomVisionPredictionEndpoint'
          value: customvisionPrediction.properties.endpoint
        }
        {
          name: 'CustomVisionTrainingKey'
          value: customvisionTraining.listKeys().key1
        }
        {
          name: 'CustomVisionTrainingEndpoint'
          value: customvisionTraining.properties.endpoint
        }
        {
          name: 'ComputerVisionKey'
          value: computervision.listKeys().key1
        }
        {
          name: 'ComputerVisionEndpoint'
          value: computervision.properties.endpoint
        }
        {
          name: 'ComputerVisionAzureEndpoint'
          value: 'https://${location}.api.cognitive.microsoft.com/vision/v3.1'
        }
        {
          name: 'CvFunctionAppEndpoint'
          value: 'https://${cvFunctionApp.properties.defaultHostName}/api'
        }
        {
          name: 'CvFunctionAppPredictionRoute'
          value: 'altitudecv'
        }
        {
          name: 'CustomVisionProjectName'
          value: customvisionProjectName
        }
        {
          name: 'CustomVisionModelName'
          value: customvisionModelName
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'StorageContainer'
          value: 'con'
        }
      ]
      minTlsVersion: '1.2'
    }
  }
}
