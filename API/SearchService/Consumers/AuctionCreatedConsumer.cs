using AutoMapper;
using Contracts;
using MassTransit;
using SearchService.Data;
using SearchService.Entities;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;
    private readonly SearchDbContext _context;

    public AuctionCreatedConsumer(IMapper mapper, SearchDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> consumeContext)
    {
        var newItem = _mapper.Map<Item>(consumeContext.Message);
        //var item = _mapper.Map<Item>(newItem);
        await _context.AddAsync(newItem);
        await _context.SaveChangesAsync();
        Console.WriteLine("--> Получение сообщения создать аукцион");
    }
}
