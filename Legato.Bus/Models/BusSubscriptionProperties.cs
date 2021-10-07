namespace Legato.Bus.Azure.Models
{
    public record BusSubscriptionProperties
    {
        public string Name { get; init; }
        public long ActiveMessageCount { get; init; }
        public long DeadLetterMessageCount { get; init; }
    }
}