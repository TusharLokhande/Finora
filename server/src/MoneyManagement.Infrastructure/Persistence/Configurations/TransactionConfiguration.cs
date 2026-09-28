using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : BaseConfiguration<Transaction>
{
    public override void Configure(EntityTypeBuilder<Transaction> builder)
    {
        base.Configure(builder);

        builder.ToTable("Transactions", t =>
        {
            t.HasCheckConstraint("CK_Transactions_AmountPositive", "\"Amount\" > 0");
            t.HasCheckConstraint("CK_Transactions_TransferShape",
                "(\"Type\" = 'Transfer' AND \"ToAccountId\" IS NOT NULL AND \"ToAccountId\" <> \"AccountId\" AND \"CategoryId\" IS NULL) " +
                "OR (\"Type\" <> 'Transfer' AND \"ToAccountId\" IS NULL AND \"CategoryId\" IS NOT NULL)");
        });

        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(200);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => new { e.UserId, e.AccountId })
            .HasPrincipalKey(e => new { e.UserId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ToAccount)
            .WithMany()
            .HasForeignKey(e => new { e.UserId, e.ToAccountId })
            .HasPrincipalKey(e => new { e.UserId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => new { e.UserId, e.CategoryId })
            .HasPrincipalKey(e => new { e.UserId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.UserId, e.TxnDate })
            .HasFilter("\"Active\" = true");

        builder.HasIndex(e => new { e.UserId, e.AccountId, e.TxnDate });
        builder.HasIndex(e => new { e.UserId, e.CategoryId, e.TxnDate });
    }
}
