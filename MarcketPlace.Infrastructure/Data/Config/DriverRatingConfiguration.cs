using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class DriverRatingConfiguration : IEntityTypeConfiguration<DriverRating>
    {
        public void Configure(EntityTypeBuilder<DriverRating> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.Score)
                   .IsRequired();

            builder.Property(x => x.Comment)
                   .HasMaxLength(1000)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.HasIndex(x => new { x.OrderId, x.DriverId, x.CustomerId })
                   .IsUnique();

            builder.HasOne(x => x.Order)
                   .WithMany(x => x.DriverRatings)
                   .HasForeignKey(x => x.OrderId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Driver)
                   .WithMany(x => x.DriverRatings)
                   .HasForeignKey(x => x.DriverId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Customer)
                   .WithMany(x => x.DriverRatings)
                   .HasForeignKey(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("DriverRatings");
        }
    }
}