using BiddingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BiddingService.Data;

public class BidDbContext : DbContext
{
    public BidDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Bid> Bids { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AuctionConfiguration());
        modelBuilder.ApplyConfiguration(new BidConfiguration());
    }
}

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.ToTable("Auctions").HasKey(p => p.Id).HasName("PK_AuctionId");
        builder.Property(p => p.Id).HasColumnType("uuid").HasColumnName("Id").IsRequired(true);
        builder.Property(p => p.AuctionEnd).HasColumnType("timestamp with time zone").HasColumnName("AuctionEnd").IsRequired(true);
        builder.Property(p => p.Seller).HasColumnType("text").HasColumnName("Seller").IsRequired(true);
        builder.Property(p => p.ReservePrice).HasColumnType("integer").HasColumnName("ReservePrice").IsRequired(true);
        builder.Property(p => p.Finished).HasColumnType("boolean").HasColumnName("Finished").IsRequired(true);
        builder.HasIndex(p => p.Id).HasDatabaseName("IX_Auctions");
    }
}


public class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.ToTable("Bids").HasKey(p => p.Id).HasName("PK_BidId");
        builder.Property(p => p.Id).HasColumnType("uuid").HasColumnName("Id").IsRequired(true);
        builder.Property(p => p.BidTime).HasColumnType("timestamp with time zone").HasColumnName("BidTime").IsRequired(true);
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(true);
        builder.Property(p => p.Bidder).HasColumnType("text").HasColumnName("Bidder").IsRequired(true);
        builder.Property(p => p.Amount).HasColumnType("integer").HasColumnName("Amount").IsRequired(true);
        builder.Property(p => p.BidStatus).HasColumnType("integer").HasColumnName("BidStatus").IsRequired(true);
        builder.HasOne(p => p.Auction).WithMany(p => p.Bids).HasForeignKey(p => p.AuctionId);
        builder.HasIndex(p => p.Id).HasDatabaseName("IX_Bids");
        builder.HasIndex(p => p.AuctionId).HasDatabaseName("IX_Bids_AuctionId");
    }
}