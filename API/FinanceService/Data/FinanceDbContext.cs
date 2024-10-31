using Common.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceService.Data;

public class FinanceDbContext : DbContext
{
    public FinanceDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<FinanceItem> FinanceItems { get; set; }

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
        builder.ToTable("FinanceItems").HasKey(p => p.Id).HasName("PK_Id");
        builder.Property(p => p.Id).HasColumnType("uuid").HasColumnName("Id").IsRequired(true);
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(false);
        builder.Property(p => p.UserLogin).HasColumnType("varchar(256)").HasColumnName("UserLogin").IsRequired(true);
        builder.Property(p => p.Value).HasColumnType("integer").HasColumnName("Value").HasPrecision(14, 2).IsRequired(true);
        builder.Property(p => p.ActionDate).HasColumnType("timestamp with time zone").HasColumnName("ActionDate").IsRequired(true);
        builder.Property(p => p.Status).HasColumnType("smallint").HasColumnName("Status").IsRequired(true);
        builder.HasIndex(p => p.Id).HasDatabaseName("PK_Items");
        builder.HasIndex(p => p.UserLogin).HasDatabaseName("IX_FinanceService_UserLogin");
        builder.HasIndex(p => p.AuctionId).HasDatabaseName("IX_FinanceService_AuctionId");
        builder.HasIndex(p => p.Status).HasDatabaseName("IX_FinanceService_Status");
    }
}
