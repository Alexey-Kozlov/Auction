using ImageService.Data;
using AutoMapper;
using Common.Contracts;
using ImageService.Entities;
using MassTransit;


namespace ImageService.Consumers;

public class AuctionCreatingImageConsumer : IConsumer<AuctionCreatingImage>
{
    private readonly IMapper _mapper;
    private readonly ImageDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionCreatingImageConsumer(IMapper mapper, ImageDbContext context, IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionCreatingImage> consumeContext)
    {
        var newItem = _mapper.Map<ImageItem>(consumeContext.Message);
        var item = _mapper.Map<ImageItem>(newItem);
        await _context.AddAsync(item);
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new AuctionCreatedImage(consumeContext.Message.CorrelationId));
        Console.WriteLine($"{DateTime.Now} Получение сообщения создать аукцион");
    }
}
