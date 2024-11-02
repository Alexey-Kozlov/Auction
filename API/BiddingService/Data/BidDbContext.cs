using Common.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BiddingService.Data;

public class BidDbContext : DbContext
{
    public BidDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<AuctionBidItem> Auctions { get; set; }
    public DbSet<BidItem> Bids { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AuctionConfiguration());
        modelBuilder.ApplyConfiguration(new BidConfiguration());
    }
}

public class AuctionConfiguration : IEntityTypeConfiguration<AuctionBidItem>
{
    public void Configure(EntityTypeBuilder<AuctionBidItem> builder)
    {
        builder.ToTable("AuctionBidItems").HasKey(p => p.AuctionId).HasName("PK_AuctionId");
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(true);
        builder.Property(p => p.AuctionEnd).HasColumnType("timestamp with time zone").HasColumnName("AuctionEnd").IsRequired(true);
        builder.Property(p => p.Seller).HasColumnType("text").HasColumnName("Seller").IsRequired(true);
        builder.Property(p => p.ReservePrice).HasColumnType("integer").HasColumnName("ReservePrice").IsRequired(true);
        builder.Property(p => p.Finished).HasColumnType("boolean").HasColumnName("Finished").IsRequired(true);
        builder.HasIndex(p => p.AuctionId).HasDatabaseName("IX_BiddingService_Auctions");
    }
}


public class BidConfiguration : IEntityTypeConfiguration<BidItem>
{
    public void Configure(EntityTypeBuilder<BidItem> builder)
    {
        builder.ToTable("BidItems").HasKey(p => p.BidId).HasName("PK_BidId");
        builder.Property(p => p.BidId).HasColumnType("uuid").HasColumnName("BidId").IsRequired(true);
        builder.Property(p => p.BidTime).HasColumnType("timestamp with time zone").HasColumnName("BidTime").IsRequired(true);
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(true);
        builder.Property(p => p.Bidder).HasColumnType("text").HasColumnName("Bidder").IsRequired(true);
        builder.Property(p => p.Amount).HasColumnType("integer").HasColumnName("Amount").IsRequired(true);
        builder.HasOne(p => p.Auction).WithMany(p => p.Bids).HasForeignKey(p => p.AuctionId);
        builder.HasIndex(p => p.BidId).HasDatabaseName("PK_Bids");
        builder.HasIndex(p => p.AuctionId).HasDatabaseName("IX_Bids_AuctionId");
    }
}