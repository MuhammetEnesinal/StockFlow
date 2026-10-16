using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Common;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StockFlow.Infrastructure.Interceptors
{
    public class AuditDbContextInterceptor(ICurrentUserService _currentUserService) : SaveChangesInterceptor
    {
        private static readonly HashSet<string> _ignoredProperties = new() { "PasswordHash", "RefreshToken", "RefreshTokenExpiresAt", "UpdateAtTime" };

        private static readonly JsonSerializerOptions _changesJsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly List<(EntityEntry<BaseEntity> Entry, AuditAction Action, string? Changes)> _pendingEntries = new();

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
                        _pendingEntries.Add((entry, AuditAction.Create, null));
                        break;

                    case EntityState.Modified:
                        var updateChanges = BuildChanges(entry, AuditAction.Update);

                        if (updateChanges != null)
                        {
                            _pendingEntries.Add((entry, AuditAction.Update, updateChanges));
                        }

                        break;

                    case EntityState.Deleted:
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
                    _pendingEntries.Clear();
                    return await base.SavedChangesAsync(eventData, result, cancellationToken);
                }

                var auditLogs = new List<AuditLog>();

                foreach (var (entry, action, changes) in _pendingEntries)
                {
                    var finalChanges = action == AuditAction.Create
                        ? BuildChanges(entry, AuditAction.Create)
                        : changes;

                    auditLogs.Add(CreateAuditLog(entry, action, finalChanges, userId, userName, employeeCode));
                }

                _pendingEntries.Clear();

                eventData.Context.Set<AuditLog>().AddRange(auditLogs);
                await eventData.Context.SaveChangesAsync(cancellationToken);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private string? BuildChanges(EntityEntry<BaseEntity> entry, AuditAction action)
        {
            var changes = new Dictionary<string, object?>();

            if (action == AuditAction.Update)
            {
                foreach (var property in entry.Properties)
                {
                    var propertyName = property.Metadata.Name;

                    if (_ignoredProperties.Contains(propertyName))
                    {
                        continue;
                    }

                    if (property.IsModified)
                    {
                        var oldValue = property.OriginalValue;
                        var newValue = property.CurrentValue;

                        if (!Equals(oldValue, newValue))
                        {
                            changes[propertyName] = new
                            {
                                Old = oldValue,
                                New = newValue
                            };
                        }
                    }
                }

                if (changes.Count == 0)
                {
                    return null;
                }
            }
            else
            {
                foreach (var property in entry.Properties)
                {
                    var propertyName = property.Metadata.Name;

                    if (_ignoredProperties.Contains(propertyName))
                    {
                        continue;
                    }

                    if (propertyName == "Id" || propertyName == "CreateAtTime" || propertyName == "IsDeleted")
                    {
                        continue;
                    }

                    if (property.Metadata.IsConcurrencyToken)
                    {
                        continue;
                    }

                    changes[propertyName] = property.CurrentValue;
                }
            }

            return JsonSerializer.Serialize(changes, _changesJsonOptions);
        }

        private AuditLog CreateAuditLog(EntityEntry<BaseEntity> entry, AuditAction action, string? changes, int userId, string userName, string employeeCode)
        {
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