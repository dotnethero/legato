using System;
using System.Linq;
using Legato.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Legato.Data.Services
{
    class QueryExecutor<T>: IQueryExecutor<T> where T : class, IReadonlyEntity
    {
        DbContext context;

        public QueryExecutor(DbContext context) => this.context = context;

        public IQueryable<T> FromSql(FormattableString query) => 
            context
                .Set<T>()
                .FromSqlInterpolated(query)
                .AsNoTrackingWithIdentityResolution();
    }
}