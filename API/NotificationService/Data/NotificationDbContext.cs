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
        builder.ToTable("NotifyItems").HasKey(p => new { p.UserLogin, p.AuctionId }).HasName("PK_NotifyUserId");
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(true);
        builder.Property(p => p.UserLogin).HasColumnType("varchar(256)").HasColumnName("UserLogin").IsRequired(true);
        builder.HasIndex("AuctionId", "UserLogin").IsUnique(true).HasDatabaseName("IX_NotifyUser_AuctionId_UserLogin");
    }
}