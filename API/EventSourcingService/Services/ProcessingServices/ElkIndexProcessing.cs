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
        try
        {
            //В процедуре Postgres делаем:
            //- запись в ES лог о переиндексации
            //Порционное получение записей всех аукционов для индексации в сервис ElasticSearchService

            //Получаем общее количество записей в БД для индексации
            //maxItemsCount = 0 - признак что нужно получить количество записей
            var result = await _dbContext.index_elk(context.Message.CorrelationId,
                context.Message.UserLogin, 0, 0).ToListAsync();
            var listItems = new DataForProcessingServicesList
            {
                DataObjects = new List<DataForProcessingService>()
            };
            //в параметре ReindexBatchSize - количество обрабатываемых записей в сообщении (в пакете)
            var batchSize = Int32.Parse(_configuration["ReindexBatchSize"]);
            //получаем количество пакетов записей
            var AllItemsCount = int.Parse(result[0].entitytype);
            int batchCount = AllItemsCount / batchSize;
            if (result.Count() % batchSize != 0)
            {
                batchCount++;
            }

            var offSet = 0;
            //формируем пакеты записей для обработки
            for (var i = 0; i < batchCount; i++)
            {
                result = await _dbContext.index_elk(context.Message.CorrelationId,
                    context.Message.UserLogin, batchSize, offSet).ToListAsync();
                //заполняем пакет записями
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
                }

                var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
                sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
                sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
                sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, AllItemsCount);
                sendObject.GetType().GetProperty("BatchCount").SetValue(sendObject, batchCount);
                await _publishEndpoint.Publish(sendObject);
                listItems.DataObjects.Clear();

                offSet += batchSize;
            }
        }
        catch (Exception e)
        {
            //ошибки прочие
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, e.Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "EventSourcingService_ElkIndex");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, context.Message.UserLogin);
            messageObject.GetType().GetProperty("AuctionId").SetValue(messageObject, context.Message.AuctionId);
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);

            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });
            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }

}