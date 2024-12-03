using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Common.Contracts.EventSourcing;
using Common.Contracts.Image;
using ImageService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Consumers;

public class SendToSetSnapShotConsumer : IConsumer<SendAllItems<SendToSetSnapShot>>
{
    private readonly IMapper _mapper;
    private readonly ImageDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public SendToSetSnapShotConsumer(IMapper mapper, ImageDbContext context,
        IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _mapper = mapper;
        _context = context;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<SendAllItems<SendToSetSnapShot>> consumeContext)
    {
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var sendImages = new SendToSetSnapShot();
        sendImages.CorrelationId = consumeContext.Message.CorrelationId;
        sendImages.SessionId = consumeContext.Message.SessionId;
        sendImages.UserLogin = consumeContext.Message.UserLogin;
        sendImages.ItemsType = nameof(ImageItem);
        sendImages.CreateAt = consumeContext.Message.CreateAt;

        var messageSize = 0;
        //получаем записи по таблице Image
        foreach (var item in await _context.Images.ToListAsync())
        {
            var imageDTO = _mapper.Map<ImageDTO>(item);
            sendImages.SnapShotItems.Add(JsonSerializer.Serialize(imageDTO, imageDTO.GetType(), options));
            messageSize += imageDTO.Image.Count();
            if ((messageSize / 1000000) >= int.Parse(_configuration["MaxMessageSizeMb"]))
            {
                await _publishEndpoint.Publish(sendImages);
                messageSize = 0;
                sendImages.SnapShotItems.Clear();
            }
        }

    }
}

