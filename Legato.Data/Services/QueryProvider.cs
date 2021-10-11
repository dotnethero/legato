using System.Linq;
using Legato.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Legato.Data.Services
{
    class QueryProvider<T>: IQueryProvider<T> where T : class, IEntity
    {
        DbContext context;

        public QueryProvider(DbContext context) => this.context = context;

        public IQueryable<T> Query() => context.Set<T>().AsNoTrackingWithIdentityResolution();
    }
}
