using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(200);
            builder.Property(x => x.Address).HasMaxLength(500);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Warehouses_Name_NotEmpty", "LEN([Name]) > 0");
                t.HasCheckConstraint("CK_Warehouses_Address_NotEmpty", "[Address] IS NULL OR LEN([Address]) > 0");
            });

            builder.HasIndex(x => x.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        }
    }
}
