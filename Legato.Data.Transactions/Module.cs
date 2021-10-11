using Autofac;
using Legato.CQRS;
using Legato.Transactions.Services;

namespace Legato.Transactions
{
    public class TransactionModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TransactionContext>().AsImplementedInterfaces().InstancePerLifetimeScope().AsSelf();

            builder.RegisterDecorator<CommandDispatcherDecorator, ICommandDispatcher>();
            builder.RegisterDecorator<EventDispatcherDecorator, IEventDispatcher>();
        }
    }
}