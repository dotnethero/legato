using System.Threading.Tasks;

namespace Legato.CQRS
{
    public interface IEventHandler<in T> where T : DomainEvent
    {
        Task Handle(T data);
    }
}
