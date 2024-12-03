using System.Reflection;
using System.Text.Json;
using Common.Contracts.EventSourcing;
using Common.Contracts.Image;
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
        switch (context.Message.EntityType)
        {
            case nameof(ESLog_ResetSnapShot):
                //Выполняем удаление всех записей в BiddingService,FinanceService,NotificationService,
                //SearchService,ImageService
                //var result1 = await _dbContext.reset_snap_shot(context.Message.CorrelationId).ToListAsync();
                _dbContext.Database.ExecuteSqlRaw("Call public.reset_snap_shot()", new object[] { });
                break;
            case nameof(RequestRestoreItems):
                //В процедуре Postgres делаем:
                //- запись в ES лог о выполнении восстановления БД из лога
                //Формирование списка корректирующих записей:
                //- набор записей о восстановлении записей ставок для сервисов BiddingService,FinanceService,NotificationService,
                //SearchService. Для ImageService - отдельно
                var result2 = await _dbContext.restore_snap_shot_items(
                    context.Message.CorrelationId,
                    context.Message.EventData,
                    context.Message.UserLogin).ToListAsync();
                //возвращаем список записей для изменения соответствующих БД в нужных сервисах
                foreach (var item in result2)
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
                break;
            case nameof(RequestRestoreImages):
                //получаем изображения из ESLog
                await GetESLogImages(context, listItems);
                return;
            default:
                break;
        }


        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
        sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
        if (context.Message.EntityType == nameof(RequestRestoreItems))
        {
            sendObject.GetType().GetProperty("BatchCount").SetValue(sendObject, -1);
        }
        await _publishEndpoint.Publish(sendObject);
    }

    private async Task GetESLogImages(ConsumeContext<ESContract> context, DataForProcessingServicesList listItems)
    {
        //максимальный размер сообщения (в МБ)
        var typedItem = JsonSerializer.Deserialize<RequestRestoreImages>(context.Message.EventData);
        typedItem.MaxMessageSizeMb = 0; //флаг - что нам нужно получить общее количество записей
        //получаем общее количество записей
        var result = await _dbContext.restore_snap_shot_images(
            context.Message.CorrelationId,
            JsonSerializer.Serialize(typedItem),
            context.Message.UserLogin).ToListAsync();
        var AllItemsCount = int.Parse(result[0].entitytype);
        typedItem = JsonSerializer.Deserialize<RequestRestoreImages>(context.Message.EventData);
        //в цикле получаем записи общим размером не превышающим MaxMessageSizeMb
        do
        {
            result = await _dbContext.restore_snap_shot_images(
            context.Message.CorrelationId,
            JsonSerializer.Serialize(typedItem),
            context.Message.UserLogin).ToListAsync();
            typedItem.StartNumber += result.Count();
            foreach (var item in result)
            {
                listItems.DataObjects.Add
                (
                    new DataForProcessingService
                    {
                        DataType = nameof(ImageItem),
                        Data = item.eventdata,
                        CRUD = 0
                    }
                );
            }
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
            sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
            sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, AllItemsCount);
            sendObject.GetType().GetProperty("BatchCount").SetValue(sendObject, typedItem.StartNumber);
            await _publishEndpoint.Publish(sendObject);
            listItems.DataObjects.Clear();

        } while (AllItemsCount > typedItem.StartNumber);
    }
}