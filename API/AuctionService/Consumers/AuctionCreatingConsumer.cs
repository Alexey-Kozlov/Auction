using AuctionService.Data;
using AuctionService.Entities;
using AuctionService.Metrics;
using AutoMapper;
using Common.Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionCreatingConsumer : IConsumer<AuctionCreating>
{
    private readonly AuctionDbContext _auctionDbContext;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly AuctionMetrics _metrics;

    public AuctionCreatingConsumer(AuctionDbContext auctionDbContext, IMapper mapper,
        IPublishEndpoint publishEndpoint, AuctionMetrics metrics)
    {
        _auctionDbContext = auctionDbContext;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _metrics = metrics;
    }
    public async Task Consume(ConsumeContext<AuctionCreating> context)
    {
        var auction = _mapper.Map<Auction>(context.Message);
        _auctionDbContext.Auctions.Add(auction);
        await _auctionDbContext.SaveChangesAsync();

        await _publishEndpoint.Publish(new AuctionCreated(context.Message.CorrelationId));
        _metrics.AddAuction();

        Console.WriteLine($"{DateTime.Now} --> Получение сообщения - создан аукцион, автор - {context.Message.AuctionAuthor}");
    }
}
