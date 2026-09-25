using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Common;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace StockFlow.Infrastructure.Interceptors
{
    public class AuditDbContextInterceptor(ICurrentUserService _currentUserService) : SaveChangesInterceptor
    {
        private readonly List<(EntityEntry<BaseEntity> Entry, AuditAction Action, string Changes)> _pendingEntries = new();

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

            _pendingEntries.Clear();

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
                        _pendingEntries.Add((entry, AuditAction.Create, BuildChanges(entry, AuditAction.Create)));
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdateAtTime = DateTime.UtcNow;
                        entry.Property(x => x.CreateAtTime).IsModified = false;

                        if (entry.Entity is User)
                        {
                            var modifiedProperties = entry.Properties
                                .Where(p => p.IsModified)
                                .Select(p => p.Metadata.Name)
                                .ToList();

                            var onlyRefreshTokenChanged = modifiedProperties.All(p =>
                                p == "RefreshToken" || p == "RefreshTokenExpiresAt" || p == "UpdateAtTime");

                            if (onlyRefreshTokenChanged)
                            {
                                break;
                            }
                        }

                        _pendingEntries.Add((entry, AuditAction.Update, BuildChanges(entry, AuditAction.Update)));
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.UpdateAtTime = DateTime.UtcNow;
                        entry.Property(x => x.CreateAtTime).IsModified = false;
                        _pendingEntries.Add((entry, AuditAction.Delete, BuildChanges(entry, AuditAction.Delete)));
                        break;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null && _pendingEntries.Any())
            {
                var auditLogs = new List<AuditLog>();

                foreach (var (entry, action, changes) in _pendingEntries)
                {
                    auditLogs.Add(CreateAuditLog(entry, action, changes));
                }

                _pendingEntries.Clear();

                eventData.Context.Set<AuditLog>().AddRange(auditLogs);
                await eventData.Context.SaveChangesAsync(cancellationToken);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private string BuildChanges(EntityEntry<BaseEntity> entry, AuditAction action)
        {
            var changes = new Dictionary<string, object?>();

            if (action == AuditAction.Update)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.IsModified)
                    {
                        var oldValue = property.OriginalValue;
                        var newValue = property.CurrentValue;

                        if (!Equals(oldValue, newValue))
                        {
                            changes[property.Metadata.Name] = new
                            {
                                Old = oldValue,
                                New = newValue
                            };
                        }
                    }
                }
            }
            else
            {
                foreach (var property in entry.Properties)
                {
                    var propertyName = property.Metadata.Name;

                    if (propertyName == "Id" || propertyName == "CreateAtTime" || propertyName == "IsDeleted" || propertyName == "UpdateAtTime")
                    {
                        continue;
                    }

                    changes[property.Metadata.Name] = property.CurrentValue;
                }
            }

            return JsonSerializer.Serialize(changes, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private AuditLog CreateAuditLog(EntityEntry<BaseEntity> entry, AuditAction action, string changes)
        {
            int userId;
            string userName;
            string employeeCode;

            try
            {
                userId = _currentUserService.GetUserId();
                userName = _currentUserService.GetUserName();
                employeeCode = _currentUserService.GetEmployeeCode();
            }
            catch
            {
                userId = 0;
                userName = "Sistem";
                employeeCode = "N/A";
            }

            return new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = entry.Entity.Id,
                Action = action,
                PerformedByUserId = userId,
                PerformedByUserName = userName,
                PerformedByEmployeeCode = employeeCode,
                CreateAtTime = DateTime.UtcNow,
                Changes = changes
            };
        }
    }
}