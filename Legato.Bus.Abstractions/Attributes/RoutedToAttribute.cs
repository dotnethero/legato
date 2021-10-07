using System;

namespace Legato.Bus.Attributes
{
    public class RoutedToAttribute : Attribute
    {
        public string Queue { get; }

        public RoutedToAttribute(string queue)
        {
            Queue = queue;
        }
    }
}
