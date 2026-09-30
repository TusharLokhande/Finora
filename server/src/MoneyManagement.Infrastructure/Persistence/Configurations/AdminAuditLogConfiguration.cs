using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Configurations;

public class AdminAuditLogConfiguration : IEntityTypeConfiguration<AdminAuditLog>
{
    public void Configure(EntityTypeBuilder<AdminAuditLog> builder)
    {
        builder.ToTable("AdminAuditLog");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Action).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.TargetEmail).HasMaxLength(320);
        builder.Property(e => e.Reason).HasMaxLength(500);
        builder.Property(e => e.CreatedAtUtc).IsRequired();

        builder.HasIndex(e => e.CreatedAtUtc);

        // Only the acting admin is an FK; admins can't be deleted from the Access page.
        // TargetUserId is deliberately not an FK so entries survive a rejected/deleted user.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.AdminUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
