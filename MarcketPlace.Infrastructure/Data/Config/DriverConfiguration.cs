using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.VehicleType)
                   .HasMaxLength(50)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.VehicleNumber)
                   .HasMaxLength(50)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.HasIndex(x => x.UserId)
                   .IsUnique();

            builder.HasOne(x => x.User)
                   .WithOne(x => x.Driver)
                   .HasForeignKey<Driver>(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Drivers");
        }
    }
}