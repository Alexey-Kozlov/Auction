using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class GetLastBidPlacedConsumer : IConsumer<GetLastBidPlaced>
{
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public GetLastBidPlacedConsumer(SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<GetLastBidPlaced> consumerContext)
    {
        Console.WriteLine($"{DateTime.Now} Получение сообщения - получить максимальную ставку по аукциону");
        var auction = await _context.Items.FirstOrDefaultAsync(p => p.Id == consumerContext.Message.Id);

        await _publishEndpoint.Publish(new GetCurrentBid(consumerContext.Message.CorrelationId, auction.CurrentHighBid));
    }
}
