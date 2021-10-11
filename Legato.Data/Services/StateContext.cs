using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Legato.Common.Contracts;
using Legato.CQRS;
using Legato.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Legato.Data.Services
{
    class StateContext : IStateContext
    {
        DbContext context;
        IEventDispatcher events;
        IDateTimeProvider time;

        public StateContext(DbContext context, IEventDispatcher events, IDateTimeProvider time)
        {
            this.context = context;
            this.context.ChangeTracker.Tracked += OnTracked;
            this.events = events;
            this.time = time;
        }

        public void Add<T>(T entity) where T : IEntity
        {
            context.Add(entity);
        }

        public void Attach<T>(T entity) where T : IEntity
        {
            context.Attach(entity);
        }

        public void Update<T>(T entity) where T : IEntity
        {
            context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove<T>(T entity) where T : IEntity
        {
            context.Remove(entity);
        }
        
        static void OnTracked(object? sender, EntityTrackedEventArgs e)
        {
            if (e.Entry.Entity is DomainEntity entity && e.Entry.State == EntityState.Added && entity.EntityId == default)
            {
                entity.EntityId = Guid.NewGuid();
            }
        }
        
        public void Store<TCommand>(TCommand command) where TCommand : DomainCommand
        {
            var entity = new DomainCommandEntity
            {
                Timestamp = time.Now,
                CorrelationId = command.CorrelationId,
                ActivityId = Activity.Current?.Id,
                Type = command.GetType().AssemblyQualifiedName!,
                Data = command
            };
            context.Add(entity);
        }

        void StoreEvent<TEvent>(TEvent domainEvent, DateTime now) where TEvent : DomainEvent
        {
            var entity = new DomainEventEntity
            {
                Timestamp = now,
                CorrelationId = domainEvent.CorrelationId,
                ActivityId = Activity.Current?.Id,
                Type = domainEvent.GetType().AssemblyQualifiedName!,
                Data = domainEvent
            };
            context.Add(entity);
        }
        
        void StoreEntityChange<TEvent>(DomainEntity entity, TEvent domainEvent, DateTime timestamp) where TEvent : DomainEvent
        {
            var change = new DomainEntityChange
            {
                Timestamp = timestamp,
                CorrelationId = domainEvent.CorrelationId,
                Type = entity.GetType().AssemblyQualifiedName!,
                EntityId = entity.EntityId,
                Version = entity.Version,
                State = entity
            };
            context.Add(change);
        }

        public async Task PublishChanges<TEvent>(TEvent domainEvent) where TEvent : DomainEvent
        {
            await InternalSaveChanges(domainEvent);
        }

        public async Task SaveChanges()
        {
            await InternalSaveChanges(null as DomainEvent);
        }
        
        async Task InternalSaveChanges<TEvent>(TEvent? domainEvent) where TEvent : DomainEvent
        {
            context.ChangeTracker.DetectChanges();

            var timestamp = time.Now;
            var changes = CollectChanges().ToArray();

            if (domainEvent is not null)
            {
                StoreEvent(domainEvent, timestamp);
            }

            await context.SaveChangesAsync();

            foreach (var (entity, state) in changes)
            {
                if (entity is DomainEntity domainEntity && domainEvent is not null)
                {
                    StoreEntityChange(domainEntity, domainEvent, timestamp);
                }

                UpdateEntityState(entity, state, timestamp);
            }

            await context.SaveChangesAsync();

            // clear context
            context.ChangeTracker.Clear();

            if (domainEvent is not null)
            {
                await events.Publish(domainEvent);
            }
        }

        static void UpdateEntityState(IEntity entity, EntityState state, DateTime timestamp)
        {
            if (entity is IHasVersion hasVersion)
            {
                hasVersion.Version++;
            }

            if (entity is IHasCreatedAt hasCreatedAt && state is EntityState.Added)
            {
                hasCreatedAt.CreatedAt = timestamp;
            }

            if (entity is IHasUpdatedAt hasUpdatedAt && state is EntityState.Added or EntityState.Modified)
            {
                hasUpdatedAt.UpdatedAt = timestamp;
            }
        }

        IEnumerable<(IEntity entity, EntityState state)> CollectChanges() =>
            from entry in context.ChangeTracker.Entries()
            where IsEntity(entry)
            where HasChanges(entry)
            let entity = (IEntity) entry.Entity
            select (entity, entry.State);

        static bool IsEntity(Type type) => type.IsAssignableFrom(typeof(IEntity));
        static bool IsEntity(EntityEntry entry) => IsEntity(entry.Metadata.ClrType);

        static bool IsNotEntity(ReferenceEntry entry) => !IsEntity(entry.Metadata.TargetEntityType.ClrType);
        static bool IsNotEntity(CollectionEntry entry) => !IsEntity(entry.Metadata.TargetEntityType.ClrType);

        static bool HasChanges(EntityEntry entry)
        {
            if (entry.State == EntityState.Added ||
                entry.State == EntityState.Modified)
                return true;

            return
                entry.Collections.Where(IsNotEntity).Any(HasChanges) ||
                entry.References.Where(IsNotEntity).Any(HasChanges);
        }

        static bool HasChanges(CollectionEntry entry) => 
            entry.IsModified;

        static bool HasChanges(ReferenceEntry entry) =>
            entry.IsModified || 
            entry.TargetEntry is not null && HasChanges(entry.TargetEntry);
    }
}
