using System.Text.Json;
using Common.Contracts.EventSourcing;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;

namespace NotificationService.Consumers;

public class SetSnapShotConsumer : IConsumer<ESContract>
{
    private readonly NotificationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public SetSnapShotConsumer(NotificationDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<ESContract> consumeContext)
    {
        var listItems = new DataForProcessingServicesList
        {
            DataObjects = new List<DataForProcessingService>()
        };

        foreach (var item in await _context.NotifyItems.ToArrayAsync())
        {
            listItems.DataObjects.Add
            (
                new DataForProcessingService
                {
                    DataType = nameof(NotifyItem),
                    Data = JsonSerializer.Serialize(item, item.GetType()),
                    CRUD = CRUD.Create
                }
            );
        }
        var sendObject = new DataForProcessingServicesList<string>
        {
            CallBackType = consumeContext.Message.CallBackType,
            DataObjects = listItems.DataObjects,
            CorrelationId = consumeContext.Message.CorrelationId,
            Props = consumeContext.Message.EventData
        };

        await _publishEndpoint.Publish(sendObject);
    }
}
