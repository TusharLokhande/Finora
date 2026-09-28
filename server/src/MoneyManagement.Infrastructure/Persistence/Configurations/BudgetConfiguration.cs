using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Configurations;

public class BudgetConfiguration : BaseConfiguration<Budget>
{
    public override void Configure(EntityTypeBuilder<Budget> builder)
    {
        base.Configure(builder);

        builder.ToTable("Budgets", t => t.HasCheckConstraint("CK_Budgets_AmountNonNegative", "\"Amount\" >= 0"));

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => new { e.UserId, e.CategoryId })
            .HasPrincipalKey(e => new { e.UserId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.CategoryId, e.Month })
            .IsUnique()
            .HasFilter("\"Active\" = true");

        builder.HasIndex(e => new { e.UserId, e.Month });
    }
}
