using Common.Contracts.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunicationService.Data;

public class CommunicationDbContext : DbContext
{
    public CommunicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<CommunicationItem> Communications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CommunicationConfiguration());
    }
}

public class CommunicationConfiguration : IEntityTypeConfiguration<CommunicationItem>
{
    public void Configure(EntityTypeBuilder<CommunicationItem> builder)
    {
        builder.ToTable("CommunicationItems").HasKey(p => new { p.ItemId, p.Commited }).HasName("PK_CommunicationItems");
        builder.Property(p => p.ItemId).HasColumnType("uuid").HasColumnName("ItemId").IsRequired(true);
        builder.Property(p => p.ParentId).HasColumnType("uuid").HasColumnName("ParentId").IsRequired(false);
        builder.Property(p => p.AuctionId).HasColumnType("uuid").HasColumnName("AuctionId").IsRequired(true);
        builder.Property(p => p.UserLogin).HasColumnType("text").HasColumnName("UserLogin").IsRequired(true);
        builder.Property(p => p.Message).HasColumnType("text").HasColumnName("Message").IsRequired(true);
        builder.Property(p => p.CreateAt).HasColumnType("timestamp with time zone").HasColumnName("CreateAt").IsRequired(true);
        builder.Property(p => p.UpdateAt).HasColumnType("timestamp with time zone").HasColumnName("UpdateAt").IsRequired(true);
        builder.Property(p => p.Commited).HasColumnType("boolean").HasColumnName("Commited").IsRequired(true);
        builder.Property(p => p.CorrelationId).HasColumnType("uuid").HasColumnName("CorrelationId").IsRequired(true);
        builder.HasIndex(p => p.ItemId).IsUnique(true).HasDatabaseName("PX_CommunicationItems");
        builder.HasIndex(p => p.AuctionId).HasDatabaseName("IX_Bids_AuctionId");
        builder.HasIndex(p => p.CorrelationId).HasDatabaseName("IX_Bids_CorrelationId");
    }
}