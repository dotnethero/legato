using System;

namespace Legato.Data.Models
{
    public interface IHasCorrelationId
    {
        public Guid CorrelationId { get; }
    }
}