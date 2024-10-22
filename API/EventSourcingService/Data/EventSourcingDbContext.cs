using EventSourcingService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventSourcingService.Data;

public class EventSourcingDbContext : DbContext
{
    public EventSourcingDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<EventsLog> EventsLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new EventsLogConfiguration());
    }

}

public class EventsLogConfiguration : IEntityTypeConfiguration<EventsLog>
{
    public void Configure(EntityTypeBuilder<EventsLog> builder)
    {
        builder.ToTable("EventsLog").HasKey(p => p.Version).HasName("PK_EventsLogId");
        builder.Property(p => p.Version).HasColumnType("integer").HasColumnName("Version").IsRequired(true).ValueGeneratedOnAdd();
        builder.Property(p => p.CorrelationId).HasColumnType("uuid").HasColumnName("CorrelationId").IsRequired(true);
        builder.Property(p => p.Commited).HasColumnType("boolean").HasColumnName("Commited").IsRequired(true);
        builder.Property(p => p.CreateAt).HasColumnType("timestamp with time zone").HasColumnName("CreateAt").IsRequired(true);
        builder.Property(p => p.EventData).HasColumnType("jsonb").HasColumnName("EventData").IsRequired(true);
        builder.Property(p => p.Info).HasColumnType("text").HasColumnName("Info").IsRequired(false);
        builder.Property(p => p.SnapShotId).HasColumnType("uuid").HasColumnName("SnapShotId").IsRequired(false);
        builder.HasIndex(p => p.Version).HasDatabaseName("PK_EventsLog");
        builder.HasIndex(p => p.CreateAt).HasDatabaseName("IX_EventsLog_CreateAt");
    }
}
