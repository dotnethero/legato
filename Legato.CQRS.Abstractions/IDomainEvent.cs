using System;

namespace Legato.CQRS
{
    public abstract record DomainEvent
    {
        public Guid CorrelationId { get; }

        protected DomainEvent() : this(Guid.NewGuid())
        {
        }
        
        protected DomainEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        protected DomainEvent(DomainCommand command)
        {
            CorrelationId = command.CorrelationId;
        }
    }
}
