using System;

namespace Legato.Bus.Attributes
{
    public class HandlesAttribute : Attribute
    {
        public string Queue { get; }

        public HandlesAttribute(string queue)
        {
            Queue = queue;
        }
    }
}