using Legato.CQRS;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Legato.Transactions.Models;

namespace Legato.Transactions.Services
{
    class TransactionContext : ITransactionContext
    {
        Func<DbContext> contextFactory;
        Func<IEventDispatcher> eventsFactory;
        Func<ICommandDispatcher> commandsFactory;
        Stack<TransactionProxy> transactions;

        public TransactionContext(
            Func<DbContext> contextFactory, 
            Func<IEventDispatcher> eventsFactory, 
            Func<ICommandDispatcher> commandsFactory)
        {
            this.contextFactory = contextFactory;
            this.eventsFactory = eventsFactory;
            this.commandsFactory = commandsFactory;
            this.transactions = new Stack<TransactionProxy>();
        }

        public async Task<ITransactionProxy> BeginTransaction()
        {
            var transaction = await contextFactory().Database.BeginTransactionAsync();
            var proxy = new TransactionProxy(this, transaction, commandsFactory(), eventsFactory());
            transactions.Push(proxy);
            return proxy;
        }

        internal bool EndTransaction(TransactionProxy proxy) =>
            transactions.TryPeek(out var peeked) &&
            peeked.Equals(proxy) &&
            transactions.TryPop(out var popped) &&
            popped.Equals(proxy);

        internal TransactionProxy CurrentTransaction =>
            transactions.TryPeek(out var transaction)
                ? transaction
                : null;
    }
}