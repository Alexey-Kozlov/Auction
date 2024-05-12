using AuctionService.Data;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionCreatingConsumer : IConsumer<AuctionCreating>
{
    private readonly AuctionDbContext _auctionDbContext;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionCreatingConsumer(AuctionDbContext auctionDbContext, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _auctionDbContext = auctionDbContext;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionCreating> context)
    {
        var auction = _mapper.Map<Auction>(context.Message);
        auction.Seller = context.Message.AuctionAuthor;
        _auctionDbContext.Auctions.Add(auction);
        await _auctionDbContext.SaveChangesAsync();

        await _publishEndpoint.Publish(new AuctionCreated(context.Message.CorrelationId));

        Console.WriteLine($"{DateTime.Now} --> Получение сообщения - создан аукцион, автор - {context.Message.AuctionAuthor}");
    }
}
