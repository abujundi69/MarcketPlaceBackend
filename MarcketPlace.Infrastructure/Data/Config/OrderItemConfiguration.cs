using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.ProductNameAr)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.ProductNameEn)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.ProductImage)
                   .HasColumnType("varbinary(max)")
                   .IsRequired(false);


            builder.Property(x => x.UnitPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.Quantity)
                   .IsRequired();

            builder.Property(x => x.LineTotal)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.HasOne(x => x.OrderStore)
                   .WithMany(x => x.OrderItems)
                   .HasForeignKey(x => x.OrderStoreId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Product)
                   .WithMany(x => x.OrderItems)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("OrderItems");
        }
    }
}