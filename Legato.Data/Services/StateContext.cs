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

        public void Add<T>(T entity) where T : DomainEntity
        {
            context.Add(entity);
        }

        public void Attach<T>(T entity) where T : DomainEntity
        {
            context.Attach(entity);
        }
        
        public void Remove<T>(T entity) where T : DomainEntity
        {
            context.Remove(entity);
        }
        
        static void OnTracked(object sender, EntityTrackedEventArgs e)
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

        public async Task PublishChanges<TEvent>(TEvent domainEvent) where TEvent : DomainEvent
        {
            context.ChangeTracker.DetectChanges();

            var timestamp = time.Now;
            var changes = CollectChanges(domainEvent, timestamp).ToArray();

            StoreEvent(domainEvent, timestamp);
            await context.SaveChangesAsync();

            foreach (var (entity, state, change) in changes)
            {
                context.Add(change);
                entity.Version++;
                if (state == EntityState.Added)
                {
                    SetCreatedAt(entity, timestamp);
                    SetUpdatedAt(entity, timestamp);
                }
                if (state == EntityState.Modified)
                {
                    SetUpdatedAt(entity, timestamp);
                }
            }

            // save domain entities changes with Ids already set
            await context.SaveChangesAsync();

            // clear context
            context.ChangeTracker.Clear();

            await events.Publish(domainEvent);
        }

        IEnumerable<(DomainEntity entity, EntityState state, DomainEntityChange change)> CollectChanges<TEvent>(TEvent e, DateTime timestamp)
            where TEvent : DomainEvent =>
            from entry in context.ChangeTracker.Entries()
            where IsDomainEntity(entry)
            where HasChanges(entry)
            let entity = (DomainEntity) entry.Entity
            select (entity, entry.State, new DomainEntityChange
            {
                Timestamp = timestamp,
                CorrelationId = e.CorrelationId,
                Type = entity.GetType().AssemblyQualifiedName!,
                EntityId = entity.EntityId,
                Version = entity.Version,
                State = entity
            });

        static bool IsDomainEntity(Type type) => type.IsSubclassOf(typeof(DomainEntity));
        static bool IsDomainEntity(EntityEntry entry) => IsDomainEntity(entry.Metadata.ClrType);

        static bool IsNotDomainEntity(ReferenceEntry entry) => !IsDomainEntity(entry.Metadata.TargetEntityType.ClrType);
        static bool IsNotDomainEntity(CollectionEntry entry) => !IsDomainEntity(entry.Metadata.TargetEntityType.ClrType);

        static bool HasChanges(EntityEntry entry)
        {
            if (entry.State == EntityState.Added ||
                entry.State == EntityState.Modified)
                return true;

            return
                entry.Collections.Where(IsNotDomainEntity).Any(HasChanges) ||
                entry.References.Where(IsNotDomainEntity).Any(HasChanges);
        }

        static bool HasChanges(CollectionEntry entry) => 
            entry.IsModified;

        static bool HasChanges(ReferenceEntry entry) =>
            entry.IsModified || 
            entry.TargetEntry is not null && HasChanges(entry.TargetEntry);

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

        static void SetCreatedAt<T>(T entity, DateTime timestamp) where T : DomainEntity
        {
            if (entity is IHasCreatedAt created)
                created.CreatedAt = timestamp;
        }

        static void SetUpdatedAt<T>(T entity, DateTime timestamp) where T : DomainEntity
        {
            if (entity is IHasUpdatedAt updated)
                updated.UpdatedAt = timestamp;
        }
    }
}
