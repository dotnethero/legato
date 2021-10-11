using System;

namespace Legato.Data.Models
{
    public abstract class DomainEntity: IEntity, IHasVersion
    {
        public Guid EntityId { get; set; }
        public int Version { get; set; }
    }
}
