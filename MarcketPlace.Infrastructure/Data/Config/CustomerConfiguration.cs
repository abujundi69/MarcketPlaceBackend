using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.HasIndex(x => x.UserId)
                   .IsUnique();
            builder.Property(x => x.DefaultAddressText)
                    .HasMaxLength(500)
                    .IsUnicode(true);

            builder.Property(x => x.DefaultLatitude)
                .HasColumnType("decimal(9,6)");

            builder.Property(x => x.DefaultLongitude)
                .HasColumnType("decimal(9,6)");

            builder.HasOne(x => x.User)
                .WithOne(x => x.Customer)
                .HasForeignKey<Customer>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DefaultDeliveryZone)
                .WithMany()
                .HasForeignKey(x => x.DefaultDeliveryZoneId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.User)
                   .WithOne(x => x.Customer)
                   .HasForeignKey<Customer>(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Customers");
        }
    }
}