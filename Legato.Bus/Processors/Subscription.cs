using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Legato.Bus.Azure.Processors
{
    abstract record Subscription(Type MessageType, Func<ServiceBusReceivedMessage, Task> Handler)
    {
    }
}
