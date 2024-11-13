using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using BiddingService.Data;
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
        foreach (var item in consumeContext.Message.Items)
        {
            //очищаем данные
            await _context.Bids.ExecuteDeleteAsync();
            //восстанавливаем тип Bid

            foreach (var bids in item.Items)
            {
                var bid = JsonSerializer.Deserialize<BidItem>(bids);
                await _context.Bids.AddAsync(bid);
                bidCounter++;
            }
        }
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new RestoreSnapShotCompleted($"Успешно восстановлено {bidCounter} записей Bid",
            Guid.NewGuid(), consumeContext.Message.UserLogin, consumeContext.Message.SessionId));
        Console.WriteLine($"{DateTime.Now} --> Восстановлено {bidCounter} записей Bid");

    }
}

