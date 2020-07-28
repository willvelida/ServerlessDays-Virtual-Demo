using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VirtualServerlessTelemetrySample.Models;

namespace VirtualServerlessTelemetrySample.Functions
{
    public class PersistMaterializedReadingsToCosmos
    {
        private readonly ILogger<PersistMaterializedReadingsToCosmos> _logger;
        private readonly IConfiguration _config;
        private CosmosClient _cosmosClient;
        private Container _container;

        public PersistMaterializedReadingsToCosmos(
            ILogger<PersistMaterializedReadingsToCosmos> logger,
            IConfiguration config,
            CosmosClient cosmosClient)
        {
            _logger = logger;
            _config = config;
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer(_config["CosmosDBDatabase"], _config["MaterializedContainer"]);
        }

        [FunctionName(nameof(PersistMaterializedReadingsToCosmos))]
        public async Task Run([CosmosDBTrigger(
            databaseName: "DeviceReadingsDB",
            collectionName: "RawReadings",
            ConnectionStringSetting = "CosmosDBConnectionString",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true,
            StartFromBeginning = true)]IReadOnlyList<Document> input)
        {
            try
            {
                if (input != null && input.Count > 0)
                {
                    foreach (var document in input)
                    {
                        var deviceReading = JsonConvert.DeserializeObject<DeviceReading>(document.ToString());

                        await _container.CreateItemAsync(document,
                            new Microsoft.Azure.Cosmos.PartitionKey(deviceReading.DeviceLocation));
                        _logger.LogInformation($"Device Reading from {deviceReading.DeviceLocation} has now been materialized!");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong. Exception thrown: {ex.Message}");
                throw;
            }
            
        }
    }
}
