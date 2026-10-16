using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StockFlow.Domain.Common;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default
         )
        {
            if (eventData.Context == null)
            {
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            var entries = eventData.Context.ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.Entity is AuditLog)
                {
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreateAtTime = DateTime.UtcNow;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdateAtTime = DateTime.UtcNow;
                        entry.Property(x => x.CreateAtTime).IsModified = false;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.UpdateAtTime = DateTime.UtcNow;
                        entry.Property(x => x.CreateAtTime).IsModified = false;
                        break;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}