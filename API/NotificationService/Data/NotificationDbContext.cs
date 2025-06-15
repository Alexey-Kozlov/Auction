using Common.Contracts.EventSourcing;
using Common.Contracts.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NotificationService.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<NotifyItem> NotifyItems { get; set; }

    public IQueryable<ReturnRestoreResultSql> notification_restore(
    Guid correlationid,
    bool reset,
    bool commit) =>
    FromExpression(() => notification_restore(correlationid, reset, commit));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new NotifyUserConfiguration());
    }
}

public class NotifyUserConfiguration : IEntityTypeConfiguration<NotifyItem>
{
    public void Configure(EntityTypeBuilder<NotifyItem> builder)
    {
        builder.ToTable("NotifyItems").HasKey(p => p.ItemId).HasName("PK_NotifyItems");
        builder.Property(p => p.ItemId).HasColumnType("uuid").HasColumnName("ItemId").IsRequired(true);
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(true);
        builder.Property(p => p.UserLogin).HasColumnType("varchar(256)").HasColumnName("UserLogin").IsRequired(true);
        builder.Property(p => p.Commited).HasColumnType("boolean").HasColumnName("Commited").IsRequired(true);
        builder.Property(p => p.CorrelationId).HasColumnType("uuid").HasColumnName("CorrelationId").IsRequired(true);
        builder.HasIndex(p => p.ItemId).IsUnique(true).HasDatabaseName("PX_NotifyItems");
        builder.HasIndex("AuctionId", "UserLogin").IsUnique(true).HasDatabaseName("IX_NotifyItems_AuctionId_UserLogin");
        builder.HasIndex(p => p.CorrelationId).IsUnique(true).HasDatabaseName("IX_NotifyItems_CorrelationId");
    }
}