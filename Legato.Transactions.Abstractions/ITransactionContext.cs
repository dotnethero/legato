using System.Threading.Tasks;

namespace Legato.Transactions
{
    public interface ITransactionContext
    {
        Task<ITransactionProxy> BeginTransaction();
    }
}
