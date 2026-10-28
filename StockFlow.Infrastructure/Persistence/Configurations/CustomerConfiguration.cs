using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Persistence.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.Property(x => x.FullName).HasMaxLength(200);
            builder.Property(x => x.Email).HasMaxLength(200);
            builder.Property(x => x.PhoneNumber).HasMaxLength(11);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Customers_FullName_NotEmpty", "LEN([FullName]) > 0");
                t.HasCheckConstraint("CK_Customers_PhoneNumber_Format", "[PhoneNumber] IS NULL OR [PhoneNumber] LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
                t.HasCheckConstraint("CK_Customers_Email_NotEmpty", "[Email] IS NULL OR LEN([Email]) > 0");
            });

            builder.HasIndex(x => x.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL AND [IsDeleted] = 0");
        }
    }
}
