using System;
using System.Threading.Tasks;
using Legato.Data.Exceptions;
using Legato.Data.Models;
using Legato.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace Legato.Data.Queries
{
    public static class Queries
    {
        public static Task<T?> Find<T, TKey>(this IQueryProvider<T> provider, TKey id)
            where T : class, IEntity, IHasId<TKey> 
            where TKey : struct =>
            provider
                .Query()
                .ById(id)
                .SingleOrDefaultAsync();
        
        public static Task<T?> Find<T>(this IQueryProvider<T> provider, Guid entityId)
            where T : DomainEntity =>
            provider
                .Query()
                .ByEntityId(entityId)
                .SingleOrDefaultAsync();

        public static async Task<T> FindRequired<T, TKey>(this IQueryProvider<T> provider, TKey id)
            where T : class, IEntity, IHasId<TKey>
            where TKey : struct =>
            await provider.Find(id) ??
            throw new EntityNotFoundException(typeof(T), (int)(object)id);

        public static async Task<T> FindRequired<T>(this IQueryProvider<T> provider, Guid entityId)
            where T : DomainEntity =>
            await provider.Query().SingleOrDefaultAsync(e => e.EntityId == entityId) ??
            throw new EntityNotFoundException(typeof(T), entityId);

        public static async Task<T> LoadRequired<T, TKey>(this IQueryProvider<T> provider, TKey id)
            where T : class, IEntity, IHasId<TKey>
            where TKey : struct =>
            await provider.Query().ById(id).AsTracking().SingleOrDefaultAsync() ??
            throw new EntityNotFoundException(typeof(T), (int)(object)id);

        public static async Task<T> LoadRequired<T>(this IQueryProvider<T> provider, Guid entityId)
            where T : DomainEntity =>
            await provider.Query().AsTracking().SingleOrDefaultAsync(e => e.EntityId == entityId) ??
            throw new EntityNotFoundException(typeof(T), entityId);
    }
}
