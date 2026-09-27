using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.BackgroundServices
{
    public class AuditLogCleanupService(IServiceScopeFactory _serviceScopeFactory) : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {


            while (!stoppingToken.IsCancellationRequested)
            {
                await  using (var scope = _serviceScopeFactory.CreateAsyncScope()) { 
                
                    var context=scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var cutoffDate = DateTime.UtcNow.AddYears(-1);

                    await context.AuditLogs
                            .Where(a => a.CreateAtTime < cutoffDate)
                            .ExecuteDeleteAsync(stoppingToken);

                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
