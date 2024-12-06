using System.Text.Json;
using BiddingService.Data;
using Common.Contracts.Bid;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Consumers;

public class SetSnapShotConsumer : IConsumer<ESContract>
{
    private readonly BidDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public SetSnapShotConsumer(BidDbContext context, IPublishEndpoint publishEndpoint,
        IConfiguration configuration)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<ESContract> consumeContext)
    {
        var listItems = new DataForProcessingServicesList
        {
            DataObjects = new List<DataForProcessingService>()
        };

        //получаем записи по таблице Bids
        foreach (var item in await _context.Bids.ToListAsync())
        {
            var rez = JsonSerializer.Serialize(item, item.GetType());
            listItems.DataObjects.Add
            (
                new DataForProcessingService
                {
                    DataType = nameof(BidItem),
                    Data = rez,
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

