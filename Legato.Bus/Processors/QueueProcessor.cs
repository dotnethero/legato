using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Legato.Bus.Azure.Processors
{
    sealed class QueueProcessor : BusMessageProcessor<QueueSubscription>
    {
        public string QueueName { get; }

        public QueueProcessor(
            string queueName, 
            IEnumerable<QueueSubscription> subscriptions, 
            ServiceBusProcessor processor, ILogger logger) : 
            base(subscriptions, processor, logger)
        {
            QueueName = queueName;
        }
    }
}