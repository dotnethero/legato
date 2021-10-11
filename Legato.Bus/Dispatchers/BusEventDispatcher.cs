using Autofac;
using Azure.Messaging.ServiceBus;
using Legato.Bus.Azure.Options;
using Legato.Bus.Extensions;
using Legato.Common.Extensions;
using Legato.CQRS.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Legato.Bus.Azure.Dispatchers
{
    class BusEventDispatcher : EventDispatcher
    {
        IOptions<BusConfiguration> options;
        ILogger<BusEventDispatcher> logger;

        public BusEventDispatcher(
            IOptions<BusConfiguration> options,
            ILifetimeScope scope, 
            ILogger<BusEventDispatcher> logger) : 
            base(scope)
        {
            this.options = options;
            this.logger = logger;
        }

        public override async Task Publish<T>(T data)
        {
            if (data.IsRouted())
            {
                await using var client = new ServiceBusClient(options.Value.ConnectionString);

                var sender = client.CreateSender(GetTopicName(data));
                var json = JsonExtensions.Serialize(data);
                var eventType = data.GetType();
                var message = new ServiceBusMessage(json)
                {
                    CorrelationId = data.CorrelationId.ToString(),
                    Subject = eventType.AssemblyQualifiedName
                };

                await sender.SendMessageAsync(message);

                var state = new Dictionary<string, object>
                {
                    {"ActivityId", Activity.Current?.Id},
                    {"Request", json }
                };

                using (logger.BeginScope(state))
                    logger.LogInformation($"ServiceBus event was sent: {eventType}");
            }
            else
            {
                await base.Publish(data);
            }
        }

        static string GetTopicName<T>(T data) => data.GetType().FullName;
    }
}
