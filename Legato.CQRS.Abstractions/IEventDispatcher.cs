using System.Threading.Tasks;

namespace Legato.CQRS
{
    public interface IEventDispatcher
    {
        Task Publish<T>(T data) where T : DomainEvent;
    }
}