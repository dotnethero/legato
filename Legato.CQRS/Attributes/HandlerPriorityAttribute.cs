using System;

namespace Legato.CQRS.Attributes
{
    public enum Priority
    {
        Lowest = -2,
        Low = -1,
        Normal = 0,
        High = 1,
        Highest = 2
    }

    public class HandlerPriorityAttribute : Attribute
    {
        public Priority Priority { get; }

        public HandlerPriorityAttribute(Priority priority)
        {
            Priority = priority;
        }
    }
}