using ImageService.Data;
using AutoMapper;
using Contracts;
using ImageService.Entities;
using MassTransit;


namespace ImageService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;
    private readonly ImageDbContext _context;

    public AuctionCreatedConsumer(IMapper mapper, ImageDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> consumeContext)
    {
        var newItem = _mapper.Map<ImageItem>(consumeContext.Message);
        var item = _mapper.Map<ImageItem>(newItem);
        await _context.AddAsync(item);
        await _context.SaveChangesAsync();
        Console.WriteLine($"{DateTime.Now} Получение сообщения создать аукцион");
    }
}
