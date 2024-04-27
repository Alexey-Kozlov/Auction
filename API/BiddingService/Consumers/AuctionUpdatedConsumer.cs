using AutoMapper;
using BiddingService.Data;
using BiddingService.Entities;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;


namespace BiddingService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;
    private readonly BidDbContext _context;

    public AuctionUpdatedConsumer(IMapper mapper, BidDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("--> Получение сообщения обновить аукцион");
        var item = await _context.Auctions.FirstOrDefaultAsync(p => p.Id == Guid.Parse(context.Message.Id));
        if (item != null)
        {
            item.AuctionEnd = context.Message.AuctionEnd;
            await _context.SaveChangesAsync();
            return;
        }
        Console.WriteLine("Ошибка обновления записи - запись " + context.Message.Id + " не найдена.");
    }
}
