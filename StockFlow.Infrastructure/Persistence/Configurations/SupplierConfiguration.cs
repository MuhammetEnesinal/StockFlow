using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(200);
            builder.Property(x => x.Email).HasMaxLength(200);
            builder.Property(x => x.PhoneNumber).HasMaxLength(11);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Suppliers_Name_NotEmpty", "LEN([Name]) > 0");
                t.HasCheckConstraint("CK_Suppliers_PhoneNumber_Format", "[PhoneNumber] IS NULL OR [PhoneNumber] LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
                t.HasCheckConstraint("CK_Suppliers_Email_NotEmpty", "[Email] IS NULL OR LEN([Email]) > 0");
            });

            builder.HasIndex(x => x.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL AND [IsDeleted] = 0");
        }
    }
}
