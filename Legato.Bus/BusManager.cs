using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Legato.Bus.Azure.Mapping;
using Legato.Bus.Azure.Models;
using Legato.Bus.Azure.Options;
using Microsoft.Extensions.Options;

namespace Legato.Bus.Azure
{
    public interface IBusManager
    {
        Task<BusQueueProperties[]> GetQueues();
        Task<BusQueueProperties> GetQueue(string queueName);
        Task<BusTopicProperties[]> GetTopics();
        Task<BusSubscriptionProperties> GetSubscription(string topicName, string subscriptionName);
    }

    class BusManager : IBusManager
    {
        ServiceBusAdministrationClient client;
        IOptions<BusConfiguration> options;

        public BusManager(IOptions<BusConfiguration> options)
        {
            this.options = options;
            this.client = new ServiceBusAdministrationClient(options.Value.ConnectionString);
        }

        public async Task<BusQueueProperties[]> GetQueues() =>
            await client
                .GetQueuesRuntimePropertiesAsync()
                .Select(q => q.MapToDto())
                .ToArrayAsync();

        public async Task<BusTopicProperties[]> GetTopics() =>
            await client
                .GetTopicsAsync()
                .SelectAwait(GetTopic)
                .ToArrayAsync();

        async ValueTask<BusTopicProperties> GetTopic(TopicProperties topic) =>
            new()
            {
                Topic = topic.Name,
                Subscriptions = await client
                    .GetSubscriptionsRuntimePropertiesAsync(topic.Name)
                    .Select(s => s.MapToDto())
                    .ToArrayAsync()
            };
        
        public async Task<BusQueueProperties> GetQueue(string queueName)
        {
            var response = await client.GetQueueRuntimePropertiesAsync(queueName);
            var queue = response.Value;
            return queue.MapToDto();
        }

        public async Task<BusSubscriptionProperties> GetSubscription(string topicName, string subscriptionName)
        {
            var response = await client.GetSubscriptionRuntimePropertiesAsync(topicName, subscriptionName);
            var subscription = response.Value;
            return subscription.MapToDto();
        }
    }
}