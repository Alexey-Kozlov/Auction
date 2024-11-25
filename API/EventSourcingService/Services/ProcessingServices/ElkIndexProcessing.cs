using System.Reflection;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class ElkIndexProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public ElkIndexProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task ProcessElkIndex(ConsumeContext<ESContract> context)
    {
        //В процедуре Postgres делаем:
        //- запись в ES лог о переиндексации
        //Формирование списка записей всех аукционов для индексации в сервис ElasticSearchService
        var result = await _dbContext.index_elk(context.Message.CorrelationId,
            context.Message.UserLogin).ToListAsync();
        var listItems = new DataForProcessingServicesList
        {
            DataObjects = new List<DataForProcessingService>()
        };
        var batchSize = Int32.Parse(_configuration["ReindexBatchSize"]);
        int batchCount = result.Count() / batchSize;
        if (result.Count() % batchSize != 0)
        {
            batchCount++;
        }

        var batchCounter = 0;
        foreach (var item in result)
        {
            listItems.DataObjects.Add
            (
                new DataForProcessingService
                {
                    DataType = "AuctionItem",
                    Data = item.eventdata,
                    CRUD = CRUD.Create
                }
            );
            batchCounter++;
            if (batchCounter > (batchSize - 1))
            {
                batchCounter = 0;
                var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
                sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
                sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
                sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, result.Count());
                sendObject.GetType().GetProperty("BatchCount").SetValue(sendObject, batchCount);
                await _publishEndpoint.Publish(sendObject);
                listItems.DataObjects.Clear();
            }
        }
        if (listItems.DataObjects.Count() > 0)
        {
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
            sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
            sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, result.Count());
            sendObject.GetType().GetProperty("BatchCount").SetValue(sendObject, batchCount);
            await _publishEndpoint.Publish(sendObject);
            listItems.DataObjects.Clear();
        }
    }

}