using Microsoft.EntityFrameworkCore;

namespace ProcessiungService.Data;

public class ProcessingDbContext : DbContext
{
    public ProcessingDbContext(DbContextOptions options) : base(options)
    {
    }
}

