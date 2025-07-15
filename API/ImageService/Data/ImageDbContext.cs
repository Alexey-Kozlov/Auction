using Common.Contracts.EventSourcing;
using Common.Contracts.Image;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImageService.Data;

public class ImageDbContext : DbContext
{
    public ImageDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<ImageItem> Images { get; set; }


    public IQueryable<ImageReturnTypeSql> get_snap_shot_images(
    int offset,
    int maxMessageSize) =>
    FromExpression(() => get_snap_shot_images(offset, maxMessageSize));

    public IQueryable<ReturnRestoreResultSql> image_restore(
    Guid correlationid,
    bool reset,
    bool commit) =>
    FromExpression(() => image_restore(correlationid, reset, commit));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
        modelBuilder.HasDbFunction(() => get_snap_shot_images(default, default));
        modelBuilder.Entity<ImageReturnTypeSql>().HasNoKey();
    }
}

public class ItemConfiguration : IEntityTypeConfiguration<ImageItem>
{
    public void Configure(EntityTypeBuilder<ImageItem> builder)
    {
        builder.ToTable("ImageItems").HasKey(p => p.ItemId).HasName("PK_ImageItems");
        builder.Property(p => p.ItemId).HasColumnType("uuid").HasColumnName("ItemId").IsRequired(true);
        builder.Property(p => p.Image).HasColumnType("bytea").HasColumnName("Image").IsRequired(true);
        builder.Property(p => p.Commited).HasColumnType("boolean").HasColumnName("Commited").IsRequired(true);
        builder.Property(p => p.CorrelationId).HasColumnType("uuid").HasColumnName("CorrelationId").IsRequired(true);
        builder.HasIndex(p => p.ItemId).IsUnique(true).HasDatabaseName("PX_ImageItems");
    }
}
