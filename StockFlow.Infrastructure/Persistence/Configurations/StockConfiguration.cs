using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.ToTable(t => t.HasCheckConstraint("CK_Stocks_Quantity_NonNegative", "[Quantity] >= 0"));

            builder.HasIndex(x => new { x.ProductId, x.WarehouseId }).IsUnique();
            builder.Property(x => x.RowVersion).IsRowVersion();

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Stocks)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Warehouse)
                .WithMany(x => x.Stocks)
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}