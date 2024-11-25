using System.Reflection;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class AuctionDeleteProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuctionDeleteProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task ProcessESLog(ConsumeContext<ESContract> context)
    {
        //В процедуре Postgres делаем:
        //- запись в ES лог об удалении аукциона
        //- запись в ES лог об удалении последнего платежа (если были ставки)
        //- записи в ES лог об удалении всех ставок (если были)
        //- записи в ES лог об удалении всех уведомлений (если были)
        //Формирование списка корректирующих записей:
        //- запись удаленного аукциона - для удаления из сервиса SearchService
        //- если были - запись удаленного платежа - для удаления из сервиса FinanceService у соответствующего пользователя
        //- если были - запись обновления денежного баланса - для обновления баланса в сервисе FinanceService у соответствующего пользователя
        //- если были - записи удаленных ставок - для удаления из сервиса BiddingService
        //- если были - записи удаленных уведомлений - для удаления из сервиса NotificationService
        var result = await _dbContext.auction_delete(
            context.Message.CorrelationId,
            context.Message.AuctionId ?? Guid.NewGuid(),
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