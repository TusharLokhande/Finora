using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : BaseConfiguration<Category>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder);

        builder.ToTable("Categories");

        builder.HasAlternateKey(e => new { e.UserId, e.Id });

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Color).HasMaxLength(20);

        // Lucide icon name stored as plain text, e.g. "utensils"; rendered via lucide-react on the client.
        builder.Property(e => e.Icon).HasMaxLength(50);

        builder.Property(e => e.DefaultKey).HasMaxLength(50);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.UserId, e.ParentId, e.Name })
            .IsUnique()
            .HasFilter("\"Active\" = true");

        builder.HasIndex(e => new { e.UserId, e.ParentId });
    }
}
