namespace Legato.Bus.Azure.Models
{
    public record BusQueueProperties
    {
        public string Name { get; init; }
        public long ActiveMessageCount { get; init; }
        public long DeadLetterMessageCount { get; init; }
        public long ScheduledMessageCount { get; init; }
    }
}