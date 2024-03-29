using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SearchService.Entities;

namespace SearchService.Data;

public class SearchDbContext : DbContext
{
    public SearchDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
    }
}

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items").HasKey(p => p.Id).HasName("PK_Id");
        builder.Property(p => p.Id).HasColumnType("uuid").HasColumnName("Id").IsRequired(true);
        builder.Property(p => p.ReservePrice).HasColumnType("integer").HasColumnName("ReservePrice").IsRequired(true);
        builder.Property(p => p.Seller).HasColumnType("text").HasColumnName("Seller").IsRequired(false);
        builder.Property(p => p.Winner).HasColumnType("text").HasColumnName("Winner").IsRequired(false);
        builder.Property(p => p.SoldAmount).HasColumnType("integer").HasColumnName("SoldAmount").IsRequired(true);
        builder.Property(p => p.CurrentHighBid).HasColumnType("integer").HasColumnName("CurrentHighBid").IsRequired(true);
        builder.Property(p => p.CreateAt).HasColumnType("timestamp with time zone").HasColumnName("CreateAt").IsRequired(true);
        builder.Property(p => p.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("UpdatedAt").IsRequired(true);
        builder.Property(p => p.AuctionEnd).HasColumnType("timestamp with time zone").HasColumnName("AuctionEnd").IsRequired(true);
        builder.Property(p => p.Status).HasColumnType("text").HasColumnName("Status").IsRequired(true);
        builder.Property(p => p.Title).HasColumnType("text").HasColumnName("Title").IsRequired(true);
        builder.Property(p => p.Properties).HasColumnType("text").HasColumnName("Properties").IsRequired(false);
        builder.Property(p => p.Description).HasColumnType("text").HasColumnName("Description").IsRequired(false);
        builder.HasIndex(p => p.Id).HasDatabaseName("PK_Items");
    }
}
