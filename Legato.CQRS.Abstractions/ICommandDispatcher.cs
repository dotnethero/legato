using System.Threading.Tasks;

namespace Legato.CQRS
{
    public interface ICommandDispatcher
    {
        Task Execute(DomainCommand command);
        Task<TResult> Execute<TResult>(DomainCommand<TResult> command);
    }
}