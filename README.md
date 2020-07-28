# Building Event-Drive Applications using Azure Cosmos DB and Azure Functions

This repository contains the demo code and slide deck used for my talk at ServerlessDays Virtual.

# Running this demo

Before running this sample, please ensure that you have:

* An Azure Event Hub namespace with an event hub called 'devicereadings'
* An Azure Cosmos DB account configured with the SQL API
* A database with 2 containers. 1 container should use /id as a partition key and the other should use /location as the partition key.