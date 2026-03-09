using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class OrderStoreConfiguration : IEntityTypeConfiguration<OrderStore>
    {
        public void Configure(EntityTypeBuilder<OrderStore> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.Note)
                   .HasMaxLength(1000)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.ReadyAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.Property(x => x.StoreSubtotal)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.HasIndex(x => new { x.OrderId, x.StoreId })
                   .IsUnique();

            builder.HasOne(x => x.Order)
                   .WithMany(x => x.OrderStores)
                   .HasForeignKey(x => x.OrderId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Store)
                   .WithMany(x => x.OrderStores)
                   .HasForeignKey(x => x.StoreId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("OrderStores");
        }
    }
}