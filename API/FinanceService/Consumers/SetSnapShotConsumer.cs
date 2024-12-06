using System.Text.Json;
using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Processing;
using FinanceService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class SetSnapShotConsumer : IConsumer<ESContract>
{
    private readonly FinanceDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public SetSnapShotConsumer(FinanceDbContext context, IPublishEndpoint publishEndpoint,
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

        //получаем записи по таблице Finance
        foreach (var item in await _context.FinanceItems.ToListAsync())
        {
            if (item.Status == FinanceRecordStatus.Баланс) continue;
            listItems.DataObjects.Add
            (
                new DataForProcessingService
                {
                    DataType = nameof(FinanceItem),
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

