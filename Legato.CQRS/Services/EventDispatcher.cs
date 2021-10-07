using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Legato.CQRS.Attributes;

namespace Legato.CQRS.Services
{
    public class EventDispatcher : IEventDispatcher
    {
        ILifetimeScope scope;

        public EventDispatcher(ILifetimeScope scope) => this.scope = scope;

        public virtual async Task Publish<T>(T data) where T : DomainEvent
        {
            static Priority GetPriority<THandler>(THandler handler) where THandler : IEventHandler<T> =>
                handler
                    .GetType()
                    .GetCustomAttribute<HandlerPriorityAttribute>()?
                    .Priority ?? Priority.Normal;

            var handlers = scope.Resolve<IEnumerable<IEventHandler<T>>>();
            foreach (var handler in handlers.OrderByDescending(GetPriority))
            {
                await handler.Handle(data);
            }
        }
    }
}
