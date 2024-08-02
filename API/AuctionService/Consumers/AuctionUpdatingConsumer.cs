using AuctionService.Data;
using AutoMapper;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Consumers;

public class AuctionUpdatingConsumer : IConsumer<AuctionUpdating>
{
    private readonly AuctionDbContext _auctionDbContext;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionUpdatingConsumer(AuctionDbContext auctionDbContext, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _auctionDbContext = auctionDbContext;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionUpdating> context)
    {
        var auction = await _auctionDbContext.Auctions.Include(p => p.Item).FirstOrDefaultAsync(p => p.Id == context.Message.Id);
        if (auction == null)
        {
            throw new Exception("Запись не найдена");
        };
        if (auction.Seller != context.Message.AuctionAuthor)
        {
            throw new Exception("Обновить аукцион может только автор аукциона");
        };

        _mapper.Map(context.Message, auction);
        await _auctionDbContext.SaveChangesAsync();
        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(context.Message));
        Console.WriteLine("--> Получение сообщения - аукцион обновлен - " + context.Message.Id);
    }
}
