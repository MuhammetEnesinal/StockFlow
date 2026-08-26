using Microsoft.EntityFrameworkCore;
using StockFlow.Domain.Common;
using StockFlow.Domain.Entities;
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


        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }  
        public DbSet<User> Users {  get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        

      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
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
