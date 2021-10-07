using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Legato.Bus.Azure.Mapping;
using Legato.Bus.Azure.Models;

namespace Legato.Bus.Azure
{
    public interface IBusMessageProvider
    {
        Task<BusMessage[]> PeekQueue(string queue, SubQueue subQueue, int maxMessages);
        Task<BusMessage[]> PeekTopic(string topic, string subscription, SubQueue subQueue, int maxMessages);
    }
    
    public enum SubQueue
    {
        None,
        DeadLetter
    }

    class BusMessageProvider : IBusMessageProvider
    {
        ServiceBusClient client;

        public BusMessageProvider(ServiceBusClient client)
        {
            this.client = client;
        }

        public async Task<BusMessage[]> PeekQueue(string queue, SubQueue subQueue, int maxMessages)
        {
            await using var receiver = CreateQueueReceiver(queue, subQueue);
            var messages = await receiver.PeekMessagesAsync(maxMessages);
            return messages.Select(msg => msg.MapToDto()).ToArray();
        }

        public async Task<BusMessage[]> PeekTopic(string topic, string subscription, SubQueue subQueue, int maxMessages)
        {
            await using var receiver = CreateSubscriptionReceiver(topic, subscription, subQueue);
            var messages = await receiver.PeekMessagesAsync(maxMessages);
            return messages.Select(msg => msg.MapToDto()).ToArray();
        }

        ServiceBusReceiver CreateQueueReceiver(string queue, SubQueue subQueue) =>
            client.CreateReceiver(
                queue, 
                GetReceiverOptions(subQueue));

        ServiceBusReceiver CreateSubscriptionReceiver(string topic, string subscription, SubQueue subQueue) =>
            client.CreateReceiver(
                topic, 
                subscription, 
                GetReceiverOptions(subQueue));

        static ServiceBusReceiverOptions GetReceiverOptions(SubQueue subQueue) =>
            new()
            {
                SubQueue = subQueue == SubQueue.DeadLetter 
                    ? global::Azure.Messaging.ServiceBus.SubQueue.DeadLetter 
                    : global::Azure.Messaging.ServiceBus.SubQueue.None
            };
    }
}