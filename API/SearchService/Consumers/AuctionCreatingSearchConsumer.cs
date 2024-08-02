using AutoMapper;
using Common.Contracts;
using MassTransit;
using SearchService.Data;
using SearchService.Entities;

namespace SearchService.Consumers;

public class AuctionCreatingSearchConsumer : IConsumer<AuctionCreatingSearch>
{
    private readonly IMapper _mapper;
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionCreatingSearchConsumer(IMapper mapper, SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionCreatingSearch> consumeContext)
    {
        var newItem = _mapper.Map<Item>(consumeContext.Message);
        await _context.AddAsync(newItem);
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new AuctionCreatedSearch(consumeContext.Message.CorrelationId));
        Console.WriteLine("--> Получение сообщения создать аукцион");
    }
}
