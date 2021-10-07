using System;

namespace Legato.Bus.Azure.Models
{
    public record BusMessage
    {
        public string Payload { get; init; }
        public string Subject { get; init; }
        public string DiagnosticId { get; init; }
        public string Exception { get; init; }
        public string Message { get; init; }
        public DateTime ScheduledEnqueueTime { get; init; }
        public DateTime EnqueuedTime { get; init; }
        public string MessageId { get; init; }
        public long SequenceNumber { get; init; }
    }
}