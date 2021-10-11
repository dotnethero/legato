using System.Threading.Tasks;
using Legato.CQRS;
using Legato.Data.Models;

namespace Legato.Data.Services
{
    public interface IStateContext
    {
        void Add<T>(T entity) where T : DomainEntity;
        void Remove<T>(T entity) where T : DomainEntity;
        void Attach<T>(T entity) where T : DomainEntity;
        void Store<TCommand>(TCommand command) where TCommand : DomainCommand;
        Task PublishChanges<TEvent>(TEvent domainEvent) where TEvent : DomainEvent;
    }
}
