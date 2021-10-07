using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Legato.Bus.Azure.Processors
{
    record TopicSubscription(Type EventType, Type ServiceType, Func<ServiceBusReceivedMessage, Task> Handler) : Subscription(EventType, Handler)
    {
        public string Topic => EventType.FullName;
        public string Queue => ServiceType.Name;
    }
}