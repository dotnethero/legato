using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Legato.Bus.Azure.Processors
{
    sealed class TopicProcessor : BusMessageProcessor<TopicSubscription>
    {
        public string Topic { get; }

        public TopicProcessor(
            string topic, 
            TopicSubscription subscription, 
            ServiceBusProcessor processor, 
            ILogger logger) : 
            base(new[] { subscription }, processor, logger)
        {
            Topic = topic;
        }
    }
}