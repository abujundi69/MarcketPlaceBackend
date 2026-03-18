using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
    {
        public void Configure(EntityTypeBuilder<OtpCode> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.UserId)
                   .IsRequired();

            builder.Property(x => x.PhoneNumber)
                   .HasMaxLength(20)
                   .IsUnicode(false)
                   .IsRequired();

            builder.Property(x => x.ExpiresAt)
                   .HasColumnType("datetime2")
                   .IsRequired();

            builder.Property(x => x.IsUsed)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.PhoneNumber);

            builder.HasIndex(x => new { x.UserId, x.IsUsed });

            builder.ToTable("OtpCodes");
        }
    }
}