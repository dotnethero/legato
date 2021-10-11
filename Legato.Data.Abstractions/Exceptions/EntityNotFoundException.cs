using System;

namespace Legato.Data.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(Type entityType, int id) : base($"'{entityType.Name}' with ID '{id}' was not found")
        {
        }

        public EntityNotFoundException(Type entityType, Guid entityId) : base($"'{entityType.Name}' with ID '{entityId}' was not found")
        {
        }

        public EntityNotFoundException(string message) : base(message)
        {
        }
    }
}
