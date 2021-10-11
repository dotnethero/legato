using System;
using System.Collections.Generic;
using System.Linq;
using Legato.Data.Models;

namespace Legato.Data.Queries
{
    public static class QueryExtensions
    {
        public static IQueryable<T> ById<T, TKey>(this IQueryable<T> query, TKey id)
            where T : IHasId<TKey>
            where TKey : struct =>
            query.Where(x => x.Id.Equals(id));

        public static IQueryable<T> ById<T, TKey>(this IQueryable<T> query, IEnumerable<TKey> ids)
            where T : IHasId<TKey>
            where TKey : struct =>
            query.Where(x => ids.Contains(x.Id));

        public static IQueryable<T> ByEntityId<T>(this IQueryable<T> query, Guid entityId)
            where T : DomainEntity =>
            query.Where(x => x.EntityId == entityId);

        public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int pageSize, int currentPage)
            where T : IEntity =>
            query.Skip(pageSize * currentPage)
                .Take(pageSize);

        public static IQueryable<T> ToIncluded<T>(this IQueryable<T> query, DateTime date)
            where T : IHasCreatedAt =>
            query.Where(x => x.CreatedAt <= date);

        public static IQueryable<T> ToExcluded<T>(this IQueryable<T> query, DateTime date)
            where T : IHasCreatedAt =>
            query.Where(x => x.CreatedAt < date);

        public static IQueryable<T> FromIncluded<T>(this IQueryable<T> query, DateTime date)
            where T : IHasCreatedAt =>
            query.Where(x => x.CreatedAt >= date);

        public static IQueryable<T> FromExcluded<T>(this IQueryable<T> query, DateTime date)
            where T : IHasCreatedAt =>
            query.Where(x => x.CreatedAt > date);

        public static IQueryable<T> ByCorrelationId<T>(this IQueryable<T> query, params Guid[] correlations)
            where T : IHasCorrelationId =>
            query.Where(x => correlations.Contains(x.CorrelationId));

        public static IQueryable<T> ByActivityId<T>(this IQueryable<T> query, params string[] activityIds)
            where T : IHasActivityId =>
            query.Where(x => activityIds.Contains(x.ActivityId));
    }
}
