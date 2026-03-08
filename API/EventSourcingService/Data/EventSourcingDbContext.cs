using Common.Contracts.EventSourcing;
using Common.Contracts.Image;
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
        string userLogin,
        byte[] image,
        bool usingimage) =>
        FromExpression(() => auction_create(correlationid, auctionid, eventdata, userLogin, image, usingimage));
    public IQueryable<ReturnResultSql> auction_delete(
        Guid correlationid,
        Guid auctionid,
        string userLogin) =>
        FromExpression(() => auction_delete(correlationid, auctionid, userLogin));
    public IQueryable<ReturnResultSql> auction_update(
        Guid correlationid,
        Guid auctionid,
        string eventdata,
        string userLogin,
        byte[] image,
        bool usingimage) =>
        FromExpression(() => auction_update(correlationid, auctionid, eventdata, userLogin, image, usingimage));

    public IQueryable<ReturnResultSql> commit_operation(
        Guid correlationid,
        bool isError) =>
        FromExpression(() => commit_operation(correlationid, isError));
    public IQueryable<ReturnResultSql> index_elk(
        Guid correlationid,
        string userLogin,
        int maxItemsCount,
        int offSet) =>
        FromExpression(() => index_elk(correlationid, userLogin, maxItemsCount, offSet));
    public IQueryable<ReturnResultSql> finance_create(
        Guid correlationid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => finance_create(correlationid, eventdata, userLogin));
    public IQueryable<ReturnResultSql> place_bid(
        Guid correlationid,
        Guid itemid,
        Guid auctionid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => place_bid(correlationid, itemid, auctionid, eventdata, userLogin));
    public IQueryable<ReturnResultSql> edit_notification(
        Guid correlationid,
        Guid itemid,
        Guid auctionid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => edit_notification(correlationid, itemid, auctionid, eventdata, userLogin));
    public IQueryable<ReturnResultSql> restore_snap_shot_items(
        Guid correlationid,
        string eventdata) =>
        FromExpression(() => restore_snap_shot_items(correlationid, eventdata));
    public IQueryable<ReturnResultSql> reset_snap_shot(
        Guid correlationid) =>
        FromExpression(() => reset_snap_shot(correlationid));
    public IQueryable<ImageReturnTypeSql> restore_snap_shot_images(
        int offset,
        int maxMessageSize,
        DateTime eventdata) =>
        FromExpression(() => restore_snap_shot_images(offset, maxMessageSize, eventdata));
    public IQueryable<ReturnResultSql> set_auction_finished(
        Guid correlationid,
        Guid auctionId) =>
        FromExpression(() => set_auction_finished(correlationid, auctionId));
    public IQueryable<ReturnResultSql> get_auction_finished(
        Guid correlationid) =>
        FromExpression(() => get_auction_finished(correlationid));
    public IQueryable<ReturnResultSql> communication_create(
        Guid correlationid,
        Guid itemid,
        Guid auctionid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => communication_create(correlationid, itemid, auctionid, eventdata, userLogin));
    public IQueryable<ReturnResultSql> communication_delete(
        Guid correlationid,
        Guid itemid,
        Guid auctionid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => communication_delete(correlationid, itemid, auctionid, eventdata, userLogin));
    public IQueryable<ReturnResultSql> communication_update(
        Guid correlationid,
        Guid itemid,
        Guid auctionid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => communication_update(correlationid, itemid, auctionid, eventdata, userLogin));
    public IQueryable<ReturnResultSql> tag_create(
        Guid correlationid,
        Guid itemid,
        Guid auctionid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => tag_create(correlationid, itemid, auctionid, eventdata, userLogin));
    public IQueryable<ReturnResultSql> tag_delete(
        Guid correlationid,
        Guid itemid,
        Guid auctionid,
        string eventdata,
        string userLogin) =>
        FromExpression(() => tag_delete(correlationid, itemid, auctionid, eventdata, userLogin));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new EventsLogConfiguration());
        modelBuilder.HasDbFunction(() => auction_create(default, default, default, default, default, default));
        modelBuilder.HasDbFunction(() => auction_update(default, default, default, default, default, default));
        modelBuilder.HasDbFunction(() => auction_delete(default, default, default));
        modelBuilder.HasDbFunction(() => finance_create(default, default, default));
        modelBuilder.HasDbFunction(() => commit_operation(default, default));
        modelBuilder.HasDbFunction(() => index_elk(default, default, default, default));
        modelBuilder.HasDbFunction(() => place_bid(default, default, default, default, default));
        modelBuilder.HasDbFunction(() => edit_notification(default, default, default, default, default));
        modelBuilder.HasDbFunction(() => set_auction_finished(default, default));
        modelBuilder.HasDbFunction(() => restore_snap_shot_items(default, default));
        modelBuilder.HasDbFunction(() => restore_snap_shot_images(default, default, default));
        modelBuilder.HasDbFunction(() => reset_snap_shot(default));
        modelBuilder.HasDbFunction(() => get_auction_finished(default));
        modelBuilder.HasDbFunction(() => communication_create(default, default, default, default, default));
        modelBuilder.HasDbFunction(() => communication_delete(default, default, default, default, default));
        modelBuilder.HasDbFunction(() => communication_update(default, default, default, default, default));
        modelBuilder.HasDbFunction(() => tag_create(default, default, default, default, default));
        modelBuilder.HasDbFunction(() => tag_delete(default, default, default, default, default));
        modelBuilder.Entity<ReturnResultSql>().HasNoKey();
        modelBuilder.Entity<ImageReturnTypeSql>().HasNoKey();
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
        builder.Property(p => p.Image).HasColumnType("bytea").HasColumnName("Image").IsRequired(false);
        builder.HasIndex(p => p.Version).HasDatabaseName("PK_EventsLog");
        builder.HasIndex(p => p.CorrelationId).HasDatabaseName("IX_EventsLog_CorrelationId");
        builder.HasIndex(p => p.AuctionId).HasDatabaseName("IX_EventsLog_AuctionId");
        builder.HasIndex(p => p.EntityType).HasDatabaseName("IX_EventsLog_EntityType");
        builder.HasIndex(p => p.UserLogin).HasDatabaseName("IX_EventsLog_UserLogin");
        builder.HasIndex(p => p.CRUD).HasDatabaseName("IX_EventsLog_CRUD");
        builder.HasIndex(p => p.SnapShotId).HasDatabaseName("IX_EventsLog_SnapShotId");
    }
}
