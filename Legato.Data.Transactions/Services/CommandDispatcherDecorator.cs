using System.Threading.Tasks;
using Legato.Bus.Extensions;
using Legato.CQRS;

namespace Legato.Transactions.Services
{
    class CommandDispatcherDecorator : ICommandDispatcher
    {
        ICommandDispatcher dispatcher;
        TransactionContext context;

        public CommandDispatcherDecorator(ICommandDispatcher dispatcher, TransactionContext context)
        {
            this.dispatcher = dispatcher;
            this.context = context;
        }

        public async Task Execute(DomainCommand command)
        {
            if (context.CurrentTransaction is not null && command.IsRoutedTo())
            {
                context.CurrentTransaction.Schedule(command);
            }
            else
            {
                await dispatcher.Execute(command);
            }
        }

        public Task<TResult> Execute<TResult>(DomainCommand<TResult> command) => dispatcher.Execute(command);
    }
}