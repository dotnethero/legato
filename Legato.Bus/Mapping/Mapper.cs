using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Legato.Bus.Azure.Models;

namespace Legato.Bus.Azure.Mapping
{
    public static class Mapper
    {
        public static BusMessage MapToDto(this ServiceBusReceivedMessage src)
        {
            var props = src.ApplicationProperties;
            props.TryGetValue(MessageProperties.DiagnosticId, out var diagnosticId);
            props.TryGetValue(MessageProperties.Message, out var message);
            props.TryGetValue(MessageProperties.Exception, out var exception);
            return new BusMessage
            {
                Payload = src.Body.ToString(),
                Subject = src.Subject,
                ScheduledEnqueueTime = src.ScheduledEnqueueTime.LocalDateTime,
                EnqueuedTime = src.EnqueuedTime.LocalDateTime,
                DiagnosticId = (string) diagnosticId,
                Message = (string) message,
                Exception = (string) exception,
                MessageId = src.MessageId,
                SequenceNumber = src.SequenceNumber
            };
        }

        public static BusQueueProperties MapToDto(this QueueRuntimeProperties src) =>
            new()
            {
                Name = src.Name,
                ActiveMessageCount = src.ActiveMessageCount,
                DeadLetterMessageCount = src.DeadLetterMessageCount,
                ScheduledMessageCount = src.ScheduledMessageCount,
            };
        
        public static BusSubscriptionProperties MapToDto(this SubscriptionRuntimeProperties src) =>
            new()
            {
                Name = src.SubscriptionName,
                ActiveMessageCount = src.ActiveMessageCount,
                DeadLetterMessageCount = src.DeadLetterMessageCount,
            };
    }
}