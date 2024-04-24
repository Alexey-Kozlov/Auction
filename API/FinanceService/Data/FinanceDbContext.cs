using FinanceService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceService.Data;

public class FinanceDbContext : DbContext
{
    public FinanceDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<BalanceItem> BalanceItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
    }
}

public class ItemConfiguration : IEntityTypeConfiguration<BalanceItem>
{
    public void Configure(EntityTypeBuilder<BalanceItem> builder)
    {
        builder.ToTable("BalanceItems").HasKey(p => p.Id).HasName("PK_Id");
        builder.Property(p => p.Id).HasColumnType("uuid").HasColumnName("Id").IsRequired(true);
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(false);
        builder.Property(p => p.UserLogin).HasColumnType("text").HasColumnName("UserLogin").IsRequired(true);
        builder.Property(p => p.Credit).HasColumnType("integer").HasColumnName("Credit").HasPrecision(14, 2).IsRequired(true);
        builder.Property(p => p.Debit).HasColumnType("integer").HasColumnName("Debit").HasPrecision(14, 2).IsRequired(true);
        builder.Property(p => p.ActionDate).HasColumnType("timestamp with time zone").HasColumnName("ActionDate").IsRequired(true);
        builder.Property(p => p.Balance).HasColumnType("integer").HasColumnName("Balance").HasPrecision(14, 2).IsRequired(true);
        builder.Property(p => p.Reserved).HasColumnType("boolean").HasColumnName("Reserved").IsRequired(true);
        builder.HasIndex(p => p.Id).HasDatabaseName("PK_Items");
        builder.HasIndex(p => p.UserLogin).HasDatabaseName("IX_FinanceService_UserLogin");
        builder.HasIndex(p => p.AuctionId).HasDatabaseName("IX_FinanceService_AuctionId");
        builder.HasIndex(p => p.ActionDate).HasDatabaseName("IX_FinanceService_ActionDate");
    }
}
