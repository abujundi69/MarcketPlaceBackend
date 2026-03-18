using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class CustomerFavoriteConfiguration : IEntityTypeConfiguration<CustomerFavorite>
    {
        public void Configure(EntityTypeBuilder<CustomerFavorite> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.HasIndex(x => new { x.CustomerId, x.ProductId })
                   .IsUnique();

            builder.HasOne(x => x.Customer)
                   .WithMany(x => x.Favorites)
                   .HasForeignKey(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Product)
                   .WithMany(x => x.CustomerFavorites)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("CustomerFavorites");
        }
    }
}
