using Common.Contracts.Auction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchService.Data;

public class SearchDbContext : DbContext
{
    public SearchDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<AuctionItem> AuctionItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
    }
}

public class ItemConfiguration : IEntityTypeConfiguration<AuctionItem>
{
    public void Configure(EntityTypeBuilder<AuctionItem> builder)
    {
        builder.ToTable("SearchItems").HasKey(p => p.AuctionId).HasName("PK_Id");
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(true);
        builder.Property(p => p.ReservePrice).HasColumnType("integer").HasColumnName("ReservePrice").IsRequired(true);
        builder.Property(p => p.Seller).HasColumnType("varchar(256)").HasColumnName("Seller").IsRequired(false);
        builder.Property(p => p.Winner).HasColumnType("varchar(256)").HasColumnName("Winner").IsRequired(false);
        builder.Property(p => p.SoldAmount).HasColumnType("integer").HasColumnName("SoldAmount").IsRequired(true);
        builder.Property(p => p.CurrentHighBid).HasColumnType("integer").HasColumnName("CurrentHighBid").IsRequired(true);
        builder.Property(p => p.CreateAt).HasColumnType("timestamp with time zone").HasColumnName("CreateAt").IsRequired(true);
        builder.Property(p => p.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("UpdatedAt").IsRequired(true);
        builder.Property(p => p.AuctionEnd).HasColumnType("timestamp with time zone").HasColumnName("AuctionEnd").IsRequired(true);
        builder.Property(p => p.Title).HasColumnType("text").HasColumnName("Title").IsRequired(true);
        builder.Property(p => p.Properties).HasColumnType("text").HasColumnName("Properties").IsRequired(false);
        builder.Property(p => p.Description).HasColumnType("text").HasColumnName("Description").IsRequired(false);
        builder.HasIndex(p => p.AuctionId).HasDatabaseName("PK_Items");
    }
}
