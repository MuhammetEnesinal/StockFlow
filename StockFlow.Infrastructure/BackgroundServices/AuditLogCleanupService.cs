using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.BackgroundServices
{
    public class AuditLogCleanupService(IServiceScopeFactory _serviceScopeFactory, ILogger<AuditLogCleanupService> _logger) : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using (var scope = _serviceScopeFactory.CreateAsyncScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var cutoffDate = DateTime.UtcNow.AddYears(-1);

                        var deletedCount = await context.AuditLogs
                                .Where(a => a.CreateAtTime < cutoffDate)
                                .ExecuteDeleteAsync(stoppingToken);

                        _logger.LogInformation("AuditLog temizliği tamamlandı. Silinen kayıt sayısı: {DeletedCount}", deletedCount);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AuditLog temizliği başarısız oldu. 24 saat sonra tekrar denenecek.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}