using ImageService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionService.Data;

public class ImageDbContext : DbContext
{
    public ImageDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<ImageItem> Images { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
    }
}

public class ItemConfiguration : IEntityTypeConfiguration<ImageItem>
{
    public void Configure(EntityTypeBuilder<ImageItem> builder)
    {
        builder.ToTable("ImageItems").HasKey(p => p.Id).HasName("PK_Id");
        builder.Property(p => p.Id).HasColumnType("uuid").HasColumnName("Id").IsRequired(true);
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(true);
        builder.Property(p => p.Image).HasColumnType("bytea").HasColumnName("Image").IsRequired(true);
        builder.HasIndex(p => p.Id).HasDatabaseName("PK_Items");
    }
}
