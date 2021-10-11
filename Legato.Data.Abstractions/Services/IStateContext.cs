using System.Threading.Tasks;
using Legato.CQRS;
using Legato.Data.Models;

namespace Legato.Data.Services
{
    public interface IStateContext
    {
        void Add<T>(T entity) where T : IEntity;
        void Attach<T>(T entity) where T : IEntity;
        void Update<T>(T entity) where T : IEntity;
        void Remove<T>(T entity) where T : IEntity;
        void Store<TCommand>(TCommand command) where TCommand : DomainCommand;
        Task PublishChanges<TEvent>(TEvent domainEvent) where TEvent : DomainEvent;
        Task SaveChanges();
    }
}
