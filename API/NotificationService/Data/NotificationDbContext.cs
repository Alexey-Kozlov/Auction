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
        builder.ToTable("NotifyItems").HasKey(p => p.Id).HasName("PK_NotifyItems");
        builder.Property(p => p.Id).HasColumnType("uuid").HasColumnName("Id").IsRequired(true);
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(true);
        builder.Property(p => p.UserLogin).HasColumnType("varchar(256)").HasColumnName("UserLogin").IsRequired(true);
        builder.Property(p => p.Commited).HasColumnType("boolean").HasColumnName("Commited").IsRequired(true);
        builder.Property(p => p.CorrelationId).HasColumnType("uuid").HasColumnName("CorrelationId").IsRequired(true);
        builder.HasIndex(p => p.Id).IsUnique(true).HasDatabaseName("PX_NotifyItems");
        builder.HasIndex("AuctionId", "UserLogin").IsUnique(true).HasDatabaseName("IX_NotifyItems_AuctionId_UserLogin");
        builder.HasIndex(p => p.CorrelationId).IsUnique(true).HasDatabaseName("IX_NotifyItems_CorrelationId");
    }
}