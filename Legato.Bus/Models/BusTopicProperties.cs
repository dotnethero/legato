namespace Legato.Bus.Azure.Models
{
    public record BusTopicProperties
    {
        public string Topic { get; init; }
        public BusSubscriptionProperties[] Subscriptions { get; init; }
    }
}