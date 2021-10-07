using System.Threading.Tasks;

namespace Legato.CQRS
{
    public interface ICommandHandler<in TCommand> where TCommand : DomainCommand
    {
        Task Handle(TCommand command);
    }

    public interface ICommandHandler<in TCommand, TResult> where TCommand : DomainCommand<TResult>
    {
        Task<TResult> Handle(TCommand command);
    }
}
