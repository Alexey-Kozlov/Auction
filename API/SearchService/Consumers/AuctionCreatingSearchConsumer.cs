using System.Reflection;
using AutoMapper;
using Common.Contracts;
using Common.Utils;
using MassTransit;
using SearchService.Data;
using SearchService.Entities;

namespace SearchService.Consumers;

public class AuctionCreatingSearchConsumer : IConsumer<AuctionCreatingSearch>
{
    private readonly IMapper _mapper;
    private readonly SearchDbContext _context;


    public AuctionCreatingSearchConsumer(IMapper mapper, SearchDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }
    public async Task Consume(ConsumeContext<AuctionCreatingSearch> context)
    {
        var newItem = _mapper.Map<Item>(context.Message);
        await _context.AddAsync(newItem);
        await _context.SaveChangesAsync();

    }
}
