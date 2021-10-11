using System.Linq;
using Legato.Data.Models;

namespace Legato.Data.Services
{
    public interface IQueryProvider<out T> where T : class, IEntity
    {
        IQueryable<T> Query();
    }
}
