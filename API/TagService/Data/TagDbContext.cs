using Common.Contracts.Tag;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TagService.Data;

public class TagDbContext : DbContext
{
    public TagDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<TagItem> TagItems { get; set; }
    public DbSet<TagList> TagLists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
        modelBuilder.ApplyConfiguration(new ItemListConfiguration());
    }
}

public class ItemConfiguration : IEntityTypeConfiguration<TagItem>
{
    public void Configure(EntityTypeBuilder<TagItem> builder)
    {
        builder.ToTable("TagItems").HasKey(p => new { p.ItemId, p.Commited }).HasName("PK_TagItems");
        builder.Property(p => p.ItemId).HasColumnType("uuid").HasColumnName("ItemId").IsRequired(true);
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(true);
        builder.Property(p => p.Name).HasColumnType("varchar(256)").HasColumnName("Name").IsRequired(true);
        builder.Property(p => p.Commited).HasColumnType("boolean").HasColumnName("Commited").IsRequired(true);
        builder.Property(p => p.CorrelationId).HasColumnType("uuid").HasColumnName("CorrelationId").IsRequired(true);
        builder.HasIndex(p => new { p.AuctionId, p.Name, p.Commited }).HasDatabaseName("PX_TagItems");
        builder.HasIndex(p => p.CorrelationId).HasDatabaseName("IX_Search_CorrelationId");

    }
}

public class ItemListConfiguration : IEntityTypeConfiguration<TagList>
{
    public void Configure(EntityTypeBuilder<TagList> builder)
    {
        builder.ToTable("TagList").HasKey(p => new { p.Name }).HasName("PK_TagList");
        builder.Property(p => p.Name).HasColumnType("varchar(256)").HasColumnName("Name").IsRequired(true);
    }
}