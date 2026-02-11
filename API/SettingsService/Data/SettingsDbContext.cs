using Common.Contracts.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SettingsService.Data;

public class SettingsDbContext : DbContext
{
    public SettingsDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<CurrentItem> Current { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new SettingsConfiguration());
    }
}

public class SettingsConfiguration : IEntityTypeConfiguration<CurrentItem>
{
    public void Configure(EntityTypeBuilder<CurrentItem> builder)
    {
        builder.ToTable("CurrentItem").HasKey(p => new { p.ItemId, p.Commited }).HasName("PK_CurrentItem");
        builder.Property(p => p.ItemId).HasColumnType("uuid").HasColumnName("ItemId").IsRequired(true);
        builder.Property(p => p.AdminMode).HasColumnType("boolean").HasColumnName("AdminMode").IsRequired(true);
        builder.Property(p => p.Commited).HasColumnType("boolean").HasColumnName("Commited").IsRequired(true);
        builder.Property(p => p.CorrelationId).HasColumnType("uuid").HasColumnName("CorrelationId").IsRequired(true);
    }
}