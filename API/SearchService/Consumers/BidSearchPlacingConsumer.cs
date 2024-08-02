﻿using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class BidSearchPlacingConsumer : IConsumer<BidSearchPlacing>
{
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public BidSearchPlacingConsumer(SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<BidSearchPlacing> consumerContext)
    {
        Console.WriteLine($"{DateTime.Now} Получение сообщения разместить заявку, автор - {consumerContext.Message.Bidder}");

        //throw new Exception("Ошибка создания ставки");
        //await Task.Delay(TimeSpan.FromSeconds(55));
        var auction = await _context.Items.FirstOrDefaultAsync(p => p.Id == consumerContext.Message.Id);
        if (consumerContext.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = consumerContext.Message.Amount;
            await _context.SaveChangesAsync();
        }
        await _publishEndpoint.Publish(new BidSearchPlaced(consumerContext.Message.CorrelationId));
    }
}
