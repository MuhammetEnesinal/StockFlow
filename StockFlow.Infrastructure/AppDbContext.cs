using Microsoft.EntityFrameworkCore;
using StockFlow.Domain.Common;
using StockFlow.Infrastructure.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) {}


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.AddInterceptors(new AuditDbContextInterceptor());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entities = modelBuilder.Model.GetEntityTypes().Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

            foreach(var entity in entities)
            {
                var method = typeof(AppDbContext).GetMethod(nameof(SetGlobalQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)?.MakeGenericMethod(entity.ClrType);
                method?.Invoke(this, new object[] { modelBuilder });
            }
            
        }

        private void SetGlobalQueryFilter<T>(ModelBuilder builder) where T : BaseEntity
        {
            builder.Entity<T>().HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
