using ElasticSearchService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElasticSearchService.Data;

public class SearchDbContext : DbContext
{
    public SearchDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }

}

