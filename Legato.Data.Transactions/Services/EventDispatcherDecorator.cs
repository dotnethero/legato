using System.Threading.Tasks;
using Legato.Bus.Extensions;
using Legato.CQRS;

namespace Legato.Transactions.Services
{
    class EventDispatcherDecorator : IEventDispatcher
    {
        IEventDispatcher dispatcher;
        TransactionContext context;

        public EventDispatcherDecorator(IEventDispatcher dispatcher, TransactionContext context)
        {
            this.dispatcher = dispatcher;
            this.context = context;
        }

        public async Task Publish<T>(T data) where T : DomainEvent
        {
            if (context.CurrentTransaction is not null && data.IsRouted())
            {
                context.CurrentTransaction.Schedule(data);
            }
            else
            {
                await dispatcher.Publish(data);
            }
        }
    }
}
