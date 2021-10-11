using System;

namespace Legato.Data.Models
{
    public class DomainEntityChange : IEntity, IHasId, IHasCorrelationId
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid EntityId { get; set; }
        public string Type { get; set; }
        public int Version { get; set; }
        public object State { get; set; }
    }
}
