using System;
using System.Threading;
using System.Threading.Tasks;

namespace Legato.Transactions
{
    public interface ITransactionProxy : IDisposable, IAsyncDisposable
    {
        public Task CommitAsync(CancellationToken cancellationToken = new());
        public Task RollbackAsync(CancellationToken cancellationToken = new());
    }
}