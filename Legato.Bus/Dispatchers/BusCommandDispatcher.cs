using Autofac;
using Azure.Messaging.ServiceBus;
using Legato.Bus.Attributes;
using Legato.Bus.Azure.Options;
using Legato.Bus.Extensions;
using Legato.Common.Extensions;
using Legato.CQRS;
using Legato.CQRS.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Legato.Bus.Azure.Dispatchers
{
    class BusCommandDispatcher : CommandDispatcher
    {
        IOptions<BusConfiguration> options;
        ILogger<BusCommandDispatcher> logger;

        public BusCommandDispatcher(
            IOptions<BusConfiguration> options, 
            ILifetimeScope scope, 
            ILogger<BusCommandDispatcher> logger) : 
            base(scope)
        {
            this.options = options;
            this.logger = logger;
        }

        public override async Task Execute(DomainCommand command)
        {
            if (command.IsRoutedTo())
            {
                await using var client = new ServiceBusClient(options.Value.ConnectionString);

                var sender = client.CreateSender(GetQueueName(command));
                var commandType = command.GetType();
                var json = JsonExtensions.Serialize(command, commandType);
                var message = new ServiceBusMessage(json)
                {
                    CorrelationId = command.CorrelationId.ToString(),
                    Subject = commandType.AssemblyQualifiedName
                };

                if (command is IHasOffset scheduled)
                {
                    message.ScheduledEnqueueTime = DateTimeOffset.UtcNow.Add(scheduled.Offset);
                }

                await sender.SendMessageAsync(message);

                var state = new Dictionary<string, object>
                {
                    {"CorrelationId", message.CorrelationId},
                    {"ActivityId", Activity.Current?.Id},
                    {"Request", json }
                };

                using (logger.BeginScope(state))
                    logger.LogInformation($"ServiceBus command was sent: {commandType}");
            }
            else if (command is not IHasOffset withOffset || withOffset.Offset == TimeSpan.Zero)
            {
                await base.Execute(command);
            }
        }

        static string GetQueueName(DomainCommand data) => 
            data
                .GetType()
                .GetCustomAttribute<RoutedToAttribute>()!.Queue;
    }
}
 