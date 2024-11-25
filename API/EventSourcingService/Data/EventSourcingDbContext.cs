using Common.Contracts.EventSourcing;
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

    public IQueryable<ReturnResultSql> auction_create(
        Guid correlationid,
        Guid auctionid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => auction_create(correlationid, auctionid, eventdata, userLogin));
    public IQueryable<ReturnResultSql> auction_delete(
        Guid correlationid,
        Guid auctionid,
        string userLogin) =>
        FromExpression(() => auction_delete(correlationid, auctionid, userLogin));
    public IQueryable<ReturnResultSql> auction_update(
        Guid correlationid,
        Guid auctionid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => auction_update(correlationid, auctionid, eventdata, userLogin));

    public IQueryable<ReturnResultSql> commit_operation(
        Guid correlationid) =>
        FromExpression(() => commit_operation(correlationid));
    public IQueryable<ReturnResultSql> index_elk(
        Guid correlationid,
        string userLogin) =>
        FromExpression(() => index_elk(correlationid, userLogin));
    public IQueryable<ReturnResultSql> finance_create(
        Guid correlationid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => finance_create(correlationid, eventdata, userLogin));
    public IQueryable<ReturnResultSql> place_bid(
        Guid correlationid,
        Guid auctionid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => place_bid(correlationid, auctionid, eventdata, userLogin));
    public IQueryable<ReturnResultSql> edit_notification(
        Guid correlationid,
        Guid auctionid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => edit_notification(correlationid, auctionid, eventdata, userLogin));
    public IQueryable<ReturnResultSql> check_auction_finished(
        Guid correlationid) =>
        FromExpression(() => check_auction_finished(correlationid));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new EventsLogConfiguration());
        modelBuilder.HasDbFunction(() => auction_create(default, default, default, default));
        modelBuilder.HasDbFunction(() => auction_update(default, default, default, default));
        modelBuilder.HasDbFunction(() => auction_delete(default, default, default));
        modelBuilder.HasDbFunction(() => finance_create(default, default, default));
        modelBuilder.HasDbFunction(() => commit_operation(default));
        modelBuilder.HasDbFunction(() => index_elk(default, default));
        modelBuilder.HasDbFunction(() => place_bid(default, default, default, default));
        modelBuilder.HasDbFunction(() => edit_notification(default, default, default, default));
        modelBuilder.HasDbFunction(() => check_auction_finished(default));
        modelBuilder.Entity<ReturnResultSql>().HasNoKey();
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
        builder.Property(p => p.Description).HasColumnType("text").HasColumnName("Description").IsRequired(false);
        builder.Property(p => p.SnapShotId).HasColumnType("uuid").HasColumnName("SnapShotId").IsRequired(false);
        builder.Property(p => p.EntityType).HasColumnType("varchar(50)").HasColumnName("EntityType").IsRequired(true);
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(false);
        builder.Property(p => p.UserLogin).HasColumnType("varchar(256)").HasColumnName("UserLogin").IsRequired(false);
        builder.Property(p => p.Command).HasColumnType("smallint").HasColumnName("Command").IsRequired(true);
        builder.Property(p => p.CRUD).HasColumnType("smallint").HasColumnName("CRUD").IsRequired(true);
        builder.HasIndex(p => p.Version).HasDatabaseName("PK_EventsLog");
        builder.HasIndex(p => p.CorrelationId).HasDatabaseName("IX_EventsLog_CorrelationId");
        builder.HasIndex(p => p.AuctionId).HasDatabaseName("IX_EventsLog_AuctionId");
        builder.HasIndex(p => p.EntityType).HasDatabaseName("IX_EventsLog_EntityType");
        builder.HasIndex(p => p.UserLogin).HasDatabaseName("IX_EventsLog_UserLogin");
        builder.HasIndex(p => p.CRUD).HasDatabaseName("IX_EventsLog_CRUD");
        builder.HasIndex(p => p.SnapShotId).HasDatabaseName("IX_EventsLog_SnapShotId");
    }
}
