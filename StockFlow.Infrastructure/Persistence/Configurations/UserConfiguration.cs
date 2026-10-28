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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.Email).HasMaxLength(200);
            builder.Property(x => x.FullName).HasMaxLength(200);
            builder.Property(x => x.EmployeeCode).HasMaxLength(20);
            builder.Property(x => x.PhoneNumber).HasMaxLength(11);
            builder.Property(x => x.PasswordHash).HasMaxLength(60);
            builder.Property(x => x.RefreshTokenHash).HasMaxLength(64);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Users_Email_NotEmpty", "LEN([Email]) > 0");
                t.HasCheckConstraint("CK_Users_FullName_NotEmpty", "LEN([FullName]) > 0");
                t.HasCheckConstraint("CK_Users_EmployeeCode_NotEmpty", "LEN([EmployeeCode]) > 0");
                t.HasCheckConstraint("CK_Users_PhoneNumber_Format", "[PhoneNumber] IS NULL OR [PhoneNumber] LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
                t.HasCheckConstraint("CK_Users_RefreshToken_Pair", "([RefreshTokenHash] IS NULL AND [RefreshTokenExpiresAt] IS NULL) OR ([RefreshTokenHash] IS NOT NULL AND [RefreshTokenExpiresAt] IS NOT NULL)");
            });

            builder.HasIndex(x => x.Email)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(x => x.EmployeeCode)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasOne(x => x.Warehouse)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
