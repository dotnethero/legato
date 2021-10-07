using System;

namespace Legato.CQRS
{
    public interface IHasOffset
    {
        TimeSpan Offset { get; }
    }

    public abstract record DomainCommand
    {
        public Guid CorrelationId { get; }

        protected DomainCommand() : this(Guid.NewGuid())
        {
        }
        
        protected DomainCommand(Guid correlationId) => CorrelationId = correlationId;
    }

    // This command is supported only within local command execution pipeline (without service bus)
    public abstract record DomainCommand<TResult> : DomainCommand
    {
        protected DomainCommand() : base(Guid.NewGuid())
        {
        }
    }
}
