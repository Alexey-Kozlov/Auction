using System.Text.Json;
using AutoMapper;
using Common.Contracts.Auction;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class SetSnapShotConsumer : IConsumer<ESContract>
{
    private readonly IMapper _mapper;
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public SetSnapShotConsumer(IMapper mapper, SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<ESContract> consumeContext)
    {
        var listItems = new DataForProcessingServicesList
        {
            DataObjects = new List<DataForProcessingService>()
        };

        var items = new List<AuctionItem>();
        _mapper.Map(await _context.AuctionItems.ToListAsync(), items);
        foreach (var item in items)
        {
            listItems.DataObjects.Add
            (
                new DataForProcessingService
                {
                    DataType = nameof(AuctionItem),
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
