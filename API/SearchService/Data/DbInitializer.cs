using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Services;

namespace SearchService;

/*это не используется в функционале, для примера выполнения сервиса в конвейере
*/
public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var _context = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
        var isSearchItems = await _context.AuctionItems.AnyAsync();
        if (!isSearchItems)
        {
            await _context.AuctionItems.AddRangeAsync();
            await _context.SaveChangesAsync();
        }
    }
}
