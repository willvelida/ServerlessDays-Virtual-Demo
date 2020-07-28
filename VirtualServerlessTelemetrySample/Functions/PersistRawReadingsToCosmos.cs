using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VirtualServerlessTelemetrySample.Models;

namespace VirtualServerlessTelemetrySample.Functions
{
    public class PersistRawReadingsToCosmos
    {
        private readonly ILogger<PersistRawReadingsToCosmos> _logger;
        private readonly IConfiguration _config;
        private CosmosClient _cosmosClient;
        private Container _deviceReadingContainer;

        public PersistRawReadingsToCosmos(
            ILogger<PersistRawReadingsToCosmos> logger,
            IConfiguration config,
            CosmosClient cosmosClient)
        {
            _logger = logger;
            _config = config;
            _cosmosClient = cosmosClient;
            _deviceReadingContainer = _cosmosClient.GetContainer(_config["CosmosDBDatabase"], _config["RawDeviceReading"]);
        }

        [FunctionName("PersistRawReadingsToCosmos")]
        public async Task Run([EventHubTrigger("devicereading", Connection = "EventHubConnectionString")] EventData[] events)
        {
            foreach (var evt in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(evt.Body.Array, evt.Body.Offset, evt.Body.Count);

                    var devieReading = JsonConvert.DeserializeObject<DeviceReading>(messageBody);

                    await _deviceReadingContainer.CreateItemAsync(
                        devieReading,
                        new PartitionKey(devieReading.DeviceId));
                    _logger.LogInformation($"Reading Device Id: {devieReading.DeviceId} has been persisted");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Something went wrong. Exception thrown: {ex.Message}");
                }
            }
        }
    }
}
