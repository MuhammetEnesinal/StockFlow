using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(x => x.OrderNumber).HasMaxLength(50);
            builder.HasIndex(x => x.OrderNumber).IsUnique();
            builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
            builder.Property(x => x.RowVersion).IsRowVersion();

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Orders_TotalAmount_Range", "[TotalAmount] >= 0 AND [TotalAmount] <= 1000000000000000");
            });

            builder.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Customer)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
