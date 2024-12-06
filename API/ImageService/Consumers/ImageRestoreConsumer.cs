using System.Reflection;
using System.Text.Json;
using AutoMapper;
using Common.Contracts.Image;
using Common.Contracts.Processing;
using Common.Utils;
using ImageService.Data;
using MassTransit;

namespace ImageService.Consumers;

public class ImageRestoreConsumer : IConsumer<DataForProcessingServicesList<ImageDTO>>
{
    private readonly ImageDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private static readonly AwaitLocker _locker = new AwaitLocker();

    public ImageRestoreConsumer(ImageDbContext context, IPublishEndpoint publishEndpoint,
        IConfiguration configuration, IMapper mapper)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<ImageDTO>> context)
    {
        await _locker.LockAsync(async () =>
        {
            var correlationId = context.Message.CorrelationId;
            foreach (var item in context.Message.DataObjects)
            {
                var typedItem = JsonSerializer.Deserialize<ImageDTO>(item.Data);
                await _context.Images.AddAsync(_mapper.Map<ImageItem>(typedItem));
                await _context.SaveChangesAsync();
            }

            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                        _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
            sendObject.GetType().GetProperty("BatchCounter").SetValue(sendObject, context.Message.DataObjects.Count());
            await _publishEndpoint.Publish(sendObject);
        });
    }
}
