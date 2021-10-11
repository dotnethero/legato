using System;

namespace Legato.Data.Exceptions
{
    public class EntityVersionMismatchException : Exception
    {
        public EntityVersionMismatchException(Type entityType, int expected, int actual) : base($"Expected {entityType.Name}.Version to be '{expected}' but was '{actual}'")
        {
        }
    }
}