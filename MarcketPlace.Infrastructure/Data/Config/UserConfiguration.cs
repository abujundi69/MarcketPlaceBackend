using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.FullName)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.PhoneNumber)
                   .HasMaxLength(20)
                   .IsUnicode(false)
                   .IsRequired();

            builder.Property(x => x.PasswordHash)
                   .HasMaxLength(500)
                   .IsUnicode(false)
                   .IsRequired();

            builder.Property(x => x.Role)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.HasIndex(x => x.PhoneNumber)
                   .IsUnique();

            builder.HasData(new User
            {
                Id = 1,
                FullName = "Super Admin",
                PhoneNumber = "0599000000",
                PasswordHash = "AQAAAAIAAYagAAAAEAARIjNEVWZ3iJmqu8zd7v9/ZpZ17wzvNtmMEZEJm816r8vP72BtUCc6/zuVpvvZPg==",
                Role = UserRole.SuperAdmin,
                IsActive = true,
                CreatedAt = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = null
            });

            builder.ToTable("Users");
        }
    }
}