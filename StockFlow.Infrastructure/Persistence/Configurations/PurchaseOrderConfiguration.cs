using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Configurations
{
    public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder.Property(x => x.PurchaseOrderNumber).HasMaxLength(50);
            builder.HasIndex(x => x.PurchaseOrderNumber).IsUnique();
            builder.Property(x => x.RowVersion).IsRowVersion();
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PurchaseOrders_ReceivedAt_RequiresSentAt", "[ReceivedAt] IS NULL OR [SentAt] IS NOT NULL");
                t.HasCheckConstraint("CK_PurchaseOrders_ReceivedAt_NotBeforeSentAt", "[ReceivedAt] IS NULL OR [SentAt] IS NULL OR [ReceivedAt] >= [SentAt]");
            });

            builder.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.PurchaseOrders)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Supplier)
                .WithMany(x => x.PurchaseOrders)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Warehouse)
                .WithMany(x => x.PurchaseOrders)
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
