using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Legato.CQRS;
using Legato.Transactions.Services;
using Microsoft.EntityFrameworkCore.Storage;

namespace Legato.Transactions.Models
{
    public sealed class TransactionProxy : ITransactionProxy
    {
        TransactionContext context;
        IDbContextTransaction transaction;
        ICommandDispatcher commandDispatcher;
        IEventDispatcher eventDispatcher;
        Queue<Func<Task>> operations;

        internal TransactionProxy(
            TransactionContext context,
            IDbContextTransaction transaction, 
            ICommandDispatcher commandDispatcher, 
            IEventDispatcher eventDispatcher)
        {
            this.context = context;
            this.transaction = transaction;
            this.commandDispatcher = commandDispatcher;
            this.eventDispatcher = eventDispatcher;
            this.operations = new Queue<Func<Task>>();
        }

        internal void Schedule(DomainCommand domainCommand) => 
            operations.Enqueue(() => Dispatch(domainCommand));

        internal void Schedule<T>(T domainEvent) 
            where T : DomainEvent => 
            operations.Enqueue(() => Dispatch(domainEvent));
        
        Task Dispatch(DomainCommand domainCommand) => 
            commandDispatcher.Execute(domainCommand);

        Task Dispatch<T>(T domainEvent) where T : DomainEvent => 
            eventDispatcher.Publish(domainEvent);

        public async Task CommitAsync(CancellationToken cancellationToken = new())
        {
            await transaction.CommitAsync(cancellationToken);
            context.EndTransaction(this);

            foreach (var operation in operations)
            {
                await operation();
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = new())
        {
            await transaction.RollbackAsync(cancellationToken);
        }

        public void Dispose()
        {
            transaction.Dispose();
            context.EndTransaction(this);
        }

        public async ValueTask DisposeAsync()
        {
            await transaction.DisposeAsync();
            context.EndTransaction(this);
        }
    }
}