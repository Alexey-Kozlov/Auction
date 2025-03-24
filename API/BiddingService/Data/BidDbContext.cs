using Common.Contracts.Bid;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BiddingService.Data;

public class BidDbContext : DbContext
{
    public BidDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<BidItem> Bids { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new BidConfiguration());
    }
}

public class BidConfiguration : IEntityTypeConfiguration<BidItem>
{
    public void Configure(EntityTypeBuilder<BidItem> builder)
    {
        builder.ToTable("BidItems").HasKey(p => p.BidId).HasName("PK_BidItems");
        builder.Property(p => p.Id).HasColumnType("uuid").HasColumnName("Id").IsRequired(true);
        builder.Property(p => p.BidId).HasColumnType("uuid").HasColumnName("BidId").IsRequired(true);
        builder.Property(p => p.BidTime).HasColumnType("timestamp with time zone").HasColumnName("BidTime").IsRequired(true);
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(true);
        builder.Property(p => p.Bidder).HasColumnType("text").HasColumnName("Bidder").IsRequired(true);
        builder.Property(p => p.Amount).HasColumnType("integer").HasColumnName("Amount").IsRequired(true);
        builder.Property(p => p.Commited).HasColumnType("boolean").HasColumnName("Commited").IsRequired(true);
        builder.Property(p => p.CorrelationId).HasColumnType("uuid").HasColumnName("CorrelationId").IsRequired(true);
        builder.HasIndex(p => p.Id).IsUnique(true).HasDatabaseName("PX_BidItems");
        builder.HasIndex(p => p.AuctionId).HasDatabaseName("IX_Bids_AuctionId");
        builder.HasIndex(p => p.CorrelationId).HasDatabaseName("IX_Bids_CorrelationId");
    }
}