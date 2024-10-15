using AutoMapper;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Entities;

namespace SearchService.Consumers;

public class TransferItemsConsumer : IConsumer<SendAllItems>
{
    private readonly IMapper _mapper;
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public TransferItemsConsumer(IMapper mapper, SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<SendAllItems> consumeContext)
    {
        var result = new AuctionItemsList();
        _mapper.Map(await _context.Items.ToListAsync(), result.AuctionItems);
        result.CorrelationId = consumeContext.Message.CorrelationId;
        await _publishEndpoint.Publish(result);
        Console.WriteLine("--> Получение сообщения передать все записи из БД");
    }
}
