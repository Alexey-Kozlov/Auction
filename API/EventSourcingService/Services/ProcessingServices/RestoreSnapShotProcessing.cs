using System.Reflection;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class RestoreSnapShotProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public RestoreSnapShotProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task ProcessESLog(ConsumeContext<ESContract> context)
    {
        var listItems = new DataForProcessingServicesList
        {
            DataObjects = new List<DataForProcessingService>()
        };
        if (context.Message.EntityType == nameof(ESLog_ResetSnapShot))
        {
            //Выполняем удаление всех записей в BiddingService,FinanceService,NotificationService,SearchService
            var result = await _dbContext.reset_snap_shot(context.Message.CorrelationId).ToListAsync();
        }
        else
        {
            //В процедуре Postgres делаем:
            //- запись в ES лог о выполнении восстановления БД из лога
            //Формирование списка корректирующих записей:
            //- набор записей о восстановлении записей ставок для сервиса BiddingService
            var result = await _dbContext.restore_snap_shot(
                context.Message.CorrelationId,
                context.Message.EventData,
                context.Message.UserLogin).ToListAsync();
            //возвращаем список записей для изменения соответствующих БД в нужных сервисах

            foreach (var item in result)
            {
                listItems.DataObjects.Add
                (
                    new DataForProcessingService
                    {
                        DataType = item.entitytype,
                        Data = item.eventdata,
                        CRUD = (CRUD)item.crud
                    }
                );
            }
        }

        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
        sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
        await _publishEndpoint.Publish(sendObject);
    }
}