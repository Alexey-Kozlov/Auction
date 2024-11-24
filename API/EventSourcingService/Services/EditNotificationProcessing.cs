using System.Reflection;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class EditNotificationProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public EditNotificationProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task ProcessESLog(ConsumeContext<ESContract> context)
    {
        //В процедуре Postgres делаем:
        //- запись в ES лог о создании / удалении уведомления для пользователя для аукциона
        //Формирование списка корректирующих записей:
        //- запись о создании / удалении уведомления в сервисе NotificationService
        var result = await _dbContext.edit_notification(
            context.Message.CorrelationId,
            context.Message.AuctionId ?? Guid.NewGuid(),
            context.Message.EventData,
            context.Message.UserLogin).ToListAsync();
        //возвращаем список записей для изменения соответствующих БД в нужных сервисах
        var listItems = new DataForProcessingServicesList
        {
            DataObjects = new List<DataForProcessingService>()
        };
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
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
        sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
        await _publishEndpoint.Publish(sendObject);
    }
}