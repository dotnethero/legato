using System;

namespace Legato.Data.Models
{
    public class DomainEventEntity : IEntity, IHasId, IHasCorrelationId, IHasActivityId
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid CorrelationId { get; set; }
        public string Type { get; set; }
        public object Data { get; set; }
        public string ActivityId { get; set; }
    }
}
