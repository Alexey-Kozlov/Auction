using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<UserFinance> UserFinances { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new UserFinanceConfiguration());
    }

    public class UserFinanceConfiguration : IEntityTypeConfiguration<UserFinance>
    {
        public void Configure(EntityTypeBuilder<UserFinance> builder)
        {
            builder.ToTable("UserFinance").HasKey(p => p.Id).HasName("PK_UserFinanceId");
            builder.Property(p => p.Id).HasColumnType("uuid").HasColumnName("Id").IsRequired(true);
            builder.Property(p => p.UserId).HasColumnType("text").HasColumnName("UserId").IsRequired(true);
            builder.Property(p => p.Credit).HasColumnType("numeric").HasColumnName("Credit").HasPrecision(14, 2).IsRequired(true);
            builder.Property(p => p.Debit).HasColumnType("numeric").HasColumnName("Debit").HasPrecision(14, 2).IsRequired(true);
            builder.Property(p => p.ActionDate).HasColumnType("timestamp with time zone").HasColumnName("ActionDate").IsRequired(true);
            builder.Property(p => p.Status).HasColumnType("integer").HasColumnName("Status").IsRequired(true);
            builder.Property(p => p.LastBalance).HasColumnType("тгьукшс").HasColumnName("LastBalance").HasPrecision(14, 2).IsRequired(true);
            builder.HasIndex(p => p.Id).HasDatabaseName("PK_UserFinance");
            builder.HasOne(p => p.ApplicationUser).WithMany(p => p.UserFinance).HasForeignKey(p => p.UserId);
            builder.HasIndex(p => p.UserId).HasDatabaseName("FK_UserFinance_ApplicationUser_UserId");
        }
    }
}
