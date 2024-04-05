using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Services;

namespace SearchService;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        var items = await httpClient.GetItemsForSearchDb();
        var _context = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
        var isSearchItems = await _context.Items.AnyAsync();
        if (items.Result.Count > 0 && !isSearchItems)
        {
            await _context.Items.AddRangeAsync(items.Result);
            await _context.SaveChangesAsync();
        }
    }
}
