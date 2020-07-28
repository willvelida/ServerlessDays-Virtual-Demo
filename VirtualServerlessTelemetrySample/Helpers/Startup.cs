using Azure.Messaging.EventHubs.Producer;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VirtualServerlessTelemetrySample.Helpers;

[assembly: FunctionsStartup(typeof(Startup))]
namespace VirtualServerlessTelemetrySample.Helpers
{
    public class Startup : FunctionsStartup
    {
        // To register services, we configure and add components to a IFunctionsHostBuilder
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Add logging
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFilter(level => true);
            });

            // Adding configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);

            var cosmosClientOptions = new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct
            };

            // Adding Cosmos Client
            builder.Services.AddSingleton((s) => new CosmosClient(config["CosmosDBConnectionString"], cosmosClientOptions));

            // Adding Event Hub client
            builder.Services.AddSingleton(s => new EventHubProducerClient(config["EventHubConnectionString"], config["EventHubName"]));
        }
    }
}
