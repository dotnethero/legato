namespace Legato.Data.Models
{
    public interface IHasId<out T>
    {
        public T Id { get; }
    }

    public interface IHasId : IHasId<int>
    {
    }
}
