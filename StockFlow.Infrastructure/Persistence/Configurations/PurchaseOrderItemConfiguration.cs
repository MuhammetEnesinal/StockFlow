using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Configurations
{
    public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
        {
            builder.Property(x => x.UnitPrice).HasPrecision(18, 2);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PurchaseOrderItems_OrderedQuantity_Range", "[OrderedQuantity] >= 1 AND [OrderedQuantity] <= 1000000");
                t.HasCheckConstraint("CK_PurchaseOrderItems_ReceivedQuantity_Range", "[ReceivedQuantity] >= 0 AND [ReceivedQuantity] <= [OrderedQuantity]");
                t.HasCheckConstraint("CK_PurchaseOrderItems_UnitPrice_Range", "[UnitPrice] > 0 AND [UnitPrice] <= 10000000");
            });

            builder.HasOne(x => x.PurchaseOrder)
                .WithMany(x => x.PurchaseOrderItems)
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.PurchaseOrderItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
