using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : BaseConfiguration<Account>
{
    public override void Configure(EntityTypeBuilder<Account> builder)
    {
        base.Configure(builder);

        builder.ToTable("Accounts", t => t.HasCheckConstraint(
            "CK_Accounts_CreditCardFields",
            "\"Type\" = 'CreditCard' OR (\"CreditLimit\" IS NULL AND \"StatementDay\" IS NULL AND \"DueDay\" IS NULL)"));

        builder.HasAlternateKey(e => new { e.UserId, e.Id });

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Color).HasMaxLength(20);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.Name })
            .IsUnique()
            .HasFilter("\"Active\" = true");
    }
}
