using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(200);
            builder.Property(x => x.SKU).HasMaxLength(50);

            builder.HasIndex(x => x.SKU)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(x => x.Price).HasPrecision(18, 2);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Products_Price_Range", "[Price] > 0 AND [Price] <= 10000000");
                t.HasCheckConstraint("CK_Products_MinimumStockLevel_Range", "[MinimumStockLevel] >= 0 AND [MinimumStockLevel] <= 1000000");
                t.HasCheckConstraint("CK_Products_Name_NotEmpty", "LEN([Name]) > 0");
                t.HasCheckConstraint("CK_Products_SKU_NotEmpty", "LEN([SKU]) > 0");
            });

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Supplier)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
