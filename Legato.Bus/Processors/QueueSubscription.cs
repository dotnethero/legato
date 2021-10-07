using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Legato.Bus.Azure.Processors
{
    record QueueSubscription(Type CommandType, string Queue, Func<ServiceBusReceivedMessage, Task> Handler) : 
        Subscription(CommandType, Handler);
}
