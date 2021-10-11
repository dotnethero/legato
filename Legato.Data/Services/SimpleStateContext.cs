using Legato.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Legato.Data.Services
{
    class SimpleStateContext : ISimpleStateContext
    {
        DbContext context;

        public SimpleStateContext(DbContext context)
        {
            this.context = context;
            this.context.ChangeTracker.Tracked += OnTracked;
        }
        
        static void OnTracked(object sender, EntityTrackedEventArgs e)
        {
            if (e.Entry.Entity is DomainEntity entity && e.Entry.State == EntityState.Added && entity.EntityId == default)
            {
                entity.EntityId = Guid.NewGuid();
            }
        }

        public void Add<T>(T entity) 
            where T : IEntity => 
            context.Add(entity);

        public void Attach<T>(T entity) 
            where T : IEntity => 
            context.Attach(entity);

        public void Modified<T>(T entity) 
            where T : IEntity => 
            context.Entry(entity).State = EntityState.Modified;

        public async Task SaveChanges()
        {
            // TODO: create generic IHasVersion interface and use LowLevelStateContext inside StateContext

            var entries = context.ChangeTracker
                .Entries()
                .Where(entry => 
                    entry.State == EntityState.Added ||
                    entry.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is DomainEntity versioned) 
                    versioned.Version++;
            }

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }
    }
}
