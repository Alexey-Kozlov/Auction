
using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class DbInit
{
    public static void InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        SeedData(scope.ServiceProvider.GetService<AuctionDbContext>());
    }

    private static void SeedData(AuctionDbContext context)
    {
        context.Database.Migrate();
        if (context.Auctions.Any()) return;
        var auctions = new List<Auction>
        {
            new() {
                Id = Guid.Parse("afbee524-5972-4075-8800-7d1f9d7b0a0c"),
                Status = Status.Начался,
                ReservePrice = 1000,
                Seller = "bob",
                AuctionEnd = DateTime.UtcNow.AddDays(100),
                Item = [new Item
                {
                    Title = "Ford GT",
                    Properties = "Цвет белый, пробег - 10000 км., год выпуска - 2000",
                    Description = "Не битый"
                }]
            }
        };
        context.AddRange(auctions);
        context.SaveChanges();
    }
}
