using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using BiddingService.Data;
using BiddingService.Entities;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Consumers;

public class RestoreSnapShotDbConsumer : IConsumer<RestoreSnapShotItems<BiddingServiceType>>
{
    private readonly BidDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public RestoreSnapShotDbConsumer(BidDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<RestoreSnapShotItems<BiddingServiceType>> consumeContext)
    {
        var bidCounter = 0;
        var auctionCounter = 0;
        foreach (var item in consumeContext.Message.Items.OrderBy(p => p.RestoringOrder))
        {
            //очищаем данные
            await _context.Bids.ExecuteDeleteAsync();
            await _context.Auctions.ExecuteDeleteAsync();
            //восстанавливаем тип Bid
            if (item.ItemsType == "Bid")
            {
                foreach (var bids in item.Items)
                {
                    var bid = JsonSerializer.Deserialize<Bid>(bids);
                    await _context.Bids.AddAsync(bid);
                    bidCounter++;
                }
            }
            //восстанавливаем тип Auction
            else
            {
                foreach (var auctions in item.Items)
                {
                    var auction = JsonSerializer.Deserialize<Auction>(auctions);
                    await _context.Auctions.AddAsync(auction);
                    auctionCounter++;
                }
            }
        }
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new RestoreSnapShotCompleted($"Успешно восстановлено {bidCounter} записей Bid и {auctionCounter} записей Auction",
            Guid.NewGuid(), consumeContext.Message.UserLogin, consumeContext.Message.SessionId));
        Console.WriteLine($"{DateTime.Now} --> Восстановлено {bidCounter} записей Bid и {auctionCounter} записей Auction");

    }
}

