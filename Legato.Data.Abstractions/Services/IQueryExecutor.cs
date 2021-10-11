using System;
using System.Linq;
using Legato.Data.Models;

namespace Legato.Data.Services
{
    public interface IQueryExecutor<out T> where T : class, IReadonlyEntity
    {
        IQueryable<T> FromSql(FormattableString query);
    }
}
