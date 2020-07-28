using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.EventHubs.Producer;
using Bogus;
using VirtualServerlessTelemetrySample.Models;
using System.Collections.Generic;
using Azure.Messaging.EventHubs;
using System.Text;

namespace VirtualServerlessTelemetrySample.Functions
{
    public class TriggerEvents
    {
        private readonly ILogger<TriggerEvents> _logger;
        private readonly EventHubProducerClient _eventHubClient;

        public TriggerEvents(
            ILogger<TriggerEvents> logger,
            EventHubProducerClient eventHubClient)
        {
            _logger = logger;
            _eventHubClient = eventHubClient;
        }

        [FunctionName(nameof(TriggerEvents))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TriggerEvents")] HttpRequest req)
        {
            IActionResult actionResult = null;

            try
            {
                var deviceReadings = new Faker<DeviceReading>()
                    .RuleFor(d => d.DeviceId, (fake) => Guid.NewGuid().ToString())
                    .RuleFor(d => d.DeviceTemperature, (fake) => Math.Round(fake.Random.Decimal(0.00m, 150.00m), 2))
                    .RuleFor(d => d.DamageLevel, (fake) => fake.PickRandom(new List<string> { "Low", "Medium", "High" }))
                    .RuleFor(d => d.DeviceAgeInDays, (fake) => fake.Random.Number(1, 120))
                    .RuleFor(d => d.DeviceLocation, (fake) => fake.PickRandom(new List<string> { "New Zealand", "Europe", "America", "South America", "Africa", "Japan", "UAE" }))
                    .Generate(100);

                foreach (var reading in deviceReadings)
                {
                    EventDataBatch eventDataBatch = await _eventHubClient.CreateBatchAsync();
                    var eventReading = JsonConvert.SerializeObject(reading);
                    eventDataBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(eventReading)));
                    await _eventHubClient.SendAsync(eventDataBatch);
                    _logger.LogInformation($"Sending reading Id: {reading.DeviceId} to Event Hub");
                }

                actionResult = new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong. Exception thrown: {ex.Message}");
                actionResult = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return actionResult;
        }
    }
}
