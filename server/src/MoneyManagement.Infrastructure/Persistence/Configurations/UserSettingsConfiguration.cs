using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("UserSettings");

        builder.HasKey(e => e.UserId);

        builder.Property(e => e.Theme).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Density).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.LastUsedType).HasConversion<string>().HasMaxLength(20);
    }
}
