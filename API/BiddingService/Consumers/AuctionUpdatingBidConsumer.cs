using AutoMapper;
using BiddingService.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;


namespace BiddingService.Consumers;

public class AuctionUpdatingBidConsumer : IConsumer<AuctionUpdatingBid>
{
    private readonly IMapper _mapper;
    private readonly BidDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionUpdatingBidConsumer(IMapper mapper, BidDbContext context, IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionUpdatingBid> context)
    {
        Console.WriteLine($"{DateTime.Now}  Получение сообщения обновить аукцион");
        var item = await _context.Auctions.FirstOrDefaultAsync(p => p.Id == context.Message.Id);
        if (item != null)
        {
            item.AuctionEnd = context.Message.AuctionEnd;
            await _context.SaveChangesAsync();
            await _publishEndpoint.Publish(new AuctionUpdatedBid(context.Message.CorrelationId));
            return;
        }
        Console.WriteLine($"{DateTime.Now} Ошибка обновления записи - запись " + context.Message.Id + " не найдена.");
        throw new Exception($"{DateTime.Now} Ошибка обновления записи - запись " + context.Message.Id + " не найдена.");
    }
}
