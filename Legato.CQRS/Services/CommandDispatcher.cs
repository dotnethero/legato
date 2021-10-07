using System;
using System.Threading.Tasks;
using Autofac;

namespace Legato.CQRS.Services
{
    public class CommandDispatcher : ICommandDispatcher
    {
        ILifetimeScope scope;

        public CommandDispatcher(ILifetimeScope scope) => this.scope = scope;

        public virtual Task Execute(DomainCommand command)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
            var handlerMethod = handlerType.GetMethod("Handle");
            if (handlerMethod == null) throw new InvalidOperationException("Handle method not found");
            var handler = scope.Resolve(handlerType);
            return (Task) handlerMethod.Invoke(handler, new object[] { command });
        }

        public virtual Task<TResult> Execute<TResult>(DomainCommand<TResult> command)
        {
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
            var handlerMethod = handlerType.GetMethod("Handle");
            if (handlerMethod == null) throw new InvalidOperationException("Handle method not found");
            var handler = scope.Resolve(handlerType);
            return (Task<TResult>) handlerMethod.Invoke(handler, new object[] { command });
        }
    }
}
