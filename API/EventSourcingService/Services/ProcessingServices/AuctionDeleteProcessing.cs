using System.Reflection;
using AuctionService.Metrics;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class AuctionDeleteProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly AuctionMetrics _auctionMetrics;
    private readonly CheckAuctionFinished _checkAuctionFinished;

    public AuctionDeleteProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration, AuctionMetrics auctionMetrics, CheckAuctionFinished checkAuctionFinished)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
        _auctionMetrics = auctionMetrics;
        _checkAuctionFinished = checkAuctionFinished;
    }

    public async Task ProcessESLog(ConsumeContext<ESContract> context)
    {
        try
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
            var auctionId = context.Message.AuctionId ?? Guid.NewGuid();
            var result = await _dbContext.auction_delete(
                context.Message.CorrelationId,
                auctionId,
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
            _auctionMetrics.DeleteAuction();
            await _checkAuctionFinished.UpdateFinishTasks(auctionId, DateTime.UtcNow, CRUD.Delete);
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
            sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
            await _publishEndpoint.Publish(sendObject);
        }
        catch (Exception e)
        {
            //ошибки прочие
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetInnerException(e).Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "EventSourcingService_AuctionDelete");
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