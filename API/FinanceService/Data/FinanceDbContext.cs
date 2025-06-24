using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceService.Data;

public class FinanceDbContext : DbContext
{
    public FinanceDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<FinanceItem> FinanceItems { get; set; }

    public IQueryable<ReturnRestoreResultSql> finance_restore(
    Guid correlationid,
    bool reset,
    bool commit) =>
    FromExpression(() => finance_restore(correlationid, reset, commit));
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
    }
}

public class ItemConfiguration : IEntityTypeConfiguration<FinanceItem>
{
    public void Configure(EntityTypeBuilder<FinanceItem> builder)
    {
        builder.ToTable("FinanceItems").HasKey(p => p.ItemId).HasName("PK_FinanceItems");
        builder.Property(p => p.ItemId).HasColumnType("uuid").HasColumnName("ItemId").IsRequired(true);
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(false);
        builder.Property(p => p.UserLogin).HasColumnType("varchar(256)").HasColumnName("UserLogin").IsRequired(true);
        builder.Property(p => p.Value).HasColumnType("integer").HasColumnName("Value").HasPrecision(14, 2).IsRequired(true);
        builder.Property(p => p.ActionDate).HasColumnType("timestamp with time zone").HasColumnName("ActionDate").IsRequired(true);
        builder.Property(p => p.Status).HasColumnType("smallint").HasColumnName("Status").IsRequired(true);
        builder.Property(p => p.Commited).HasColumnType("boolean").HasColumnName("Commited").IsRequired(true);
        builder.Property(p => p.CorrelationId).HasColumnType("uuid").HasColumnName("CorrelationId").IsRequired(true);
        builder.HasIndex(p => p.ItemId).IsUnique(true).HasDatabaseName("PX_FinanceItems");
        builder.HasIndex(p => p.UserLogin).HasDatabaseName("IX_Finance_UserLogin");
        builder.HasIndex(p => p.AuctionId).HasDatabaseName("IX_Finance_AuctionId");
        builder.HasIndex(p => p.CorrelationId).HasDatabaseName("IX_Finance_CorrelationId");
    }
}
