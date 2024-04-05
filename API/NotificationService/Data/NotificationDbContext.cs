using NotificationService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NotificationService.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<NotifyUser> NotifyUser { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new NotifyUserConfiguration());
    }
}

public class NotifyUserConfiguration : IEntityTypeConfiguration<NotifyUser>
{
    public void Configure(EntityTypeBuilder<NotifyUser> builder)
    {
        builder.ToTable("NotifyUser").HasKey(p => new { p.UserLogin, p.AuctionId }).HasName("PK_NotifyUserId");
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(true);
        builder.Property(p => p.UserLogin).HasColumnType("text").HasColumnName("UserLogin").IsRequired(true);
        builder.HasIndex("AuctionId", "UserLogin").IsUnique(true).HasDatabaseName("IX_NotifyUser_AuctionId_UserLogin");
    }
}