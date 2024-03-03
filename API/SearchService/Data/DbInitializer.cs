using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(
            app.Configuration.GetConnectionString("MongoDbConnection")
        ));

        //индексы для поиска
        await DB.Index<Item>()
            .Key(p => p.Make, KeyType.Text)
            .Key(p => p.Model, KeyType.Text)
            .Key(p => p.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Item>();
        using var scope = app.Services.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        var items = await httpClient.GetItemsForSearchDb();
        if(items.Count > 0) await DB.SaveAsync(items);
    }
}
