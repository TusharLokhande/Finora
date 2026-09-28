using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Email).HasColumnType("citext").HasMaxLength(320).IsRequired();
        builder.Property(e => e.PasswordHash).IsRequired();
        builder.Property(e => e.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.RejectionReason).HasMaxLength(500);
        builder.Property(e => e.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(e => e.Timezone).HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAtUtc).IsRequired();

        builder.HasIndex(e => e.Email).IsUnique();

        builder.HasOne(e => e.Settings)
            .WithOne(e => e.User)
            .HasForeignKey<UserSettings>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
