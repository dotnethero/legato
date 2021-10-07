using System;
using System.Threading;
using System.Threading.Tasks;

namespace Legato.Transactions
{
    public interface ITransactionContext
    {
        Task<TransactionProxy> BeginTransaction();
    }

    public abstract class TransactionProxy : IDisposable, IAsyncDisposable
    {
        public abstract Task CommitAsync(CancellationToken cancellationToken = new());
        public abstract Task RollbackAsync(CancellationToken cancellationToken = new());
        public abstract void Dispose();
        public abstract ValueTask DisposeAsync();
    }
}
