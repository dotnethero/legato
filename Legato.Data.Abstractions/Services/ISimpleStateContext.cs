using System.Threading.Tasks;
using Legato.Data.Models;

namespace Legato.Data.Services
{
    public interface ISimpleStateContext
    {
        void Add<T>(T entity) where T : IEntity;
        void Attach<T>(T entity) where T : IEntity;
        void Modified<T>(T entity) where T : IEntity;
        Task SaveChanges();
    }
}
