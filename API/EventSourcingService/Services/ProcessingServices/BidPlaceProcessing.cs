using System.Reflection;
using AuctionService.Metrics;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class BidPlaceProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly AuctionMetrics _auctionMetrics;

    public BidPlaceProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration, AuctionMetrics auctionMetrics)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
        _auctionMetrics = auctionMetrics;
    }

    public async Task ProcessESLog(ConsumeContext<ESContract> context)
    {
        var listItems = new DataForProcessingServicesList
        {
            DataObjects = new List<DataForProcessingService>()
        };
        try
        {
            //В процедуре Postgres делаем:
            //- запись в ES лог о создании ставки
            //- записи в ES лог об отмене ранее созданного платежа (если был)
            //Формирование списка корректирующих записей:
            //- запись созданной ставки - для добавления в сервис BiddingService
            //- запись об отмене ранее созданного платежа (если был) - для отмены в сервисе FinanceService
            //- запись о новом балансе пользователя, который сделал прежнюю ставку (если было) - для обновления баланса в сервисе FinanceService
            //- запись о новом балансе пользователя, который сделал новую ставку - для обновления баланса в сервисе FinanceService        
            //- запись о списании денег на новую ставку для текущего пользователя - для сервиса FinanceService
            var result = await _dbContext.place_bid(
                context.Message.CorrelationId,
                context.Message.ItemId.Value,
                context.Message.AuctionId.Value,
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
            _auctionMetrics.BidAuction();
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
            sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
            await _publishEndpoint.Publish(sendObject);
        }
        catch (Npgsql.PostgresException e)
        {
            //ошибка при выполнении транзакции в БД, в т.ч. штатные - при нехватке денег на ставку
            Fault<ESLogPlaceBid> errorObj = new FaultMessage<ESLogPlaceBid>(
                new ESLogPlaceBid
                {
                    CorrelationId = context.Message.CorrelationId,
                    ErrorServiceName = "EventSourcingService_BidPlace",
                    ErrorMessage = e.MessageText,
                    ErrorExceptionStack = e.StackTrace,
                    ErrorExceptionInputData = GetErrorMessage.GetExceptionStringData(context.Message),
                    UserLogin = context.Message.UserLogin,
                    AuctionId = context.Message.AuctionId,
                }
            );
            await _publishEndpoint.Publish(errorObj);
        }
        catch (Exception e)
        {
            //ошибки прочие
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetMessage(e));
            messageObject.GetType().GetProperty("ErrorExceptionStack").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorExceptionInputData").SetValue(messageObject, GetErrorMessage.GetExceptionStringData(context.Message));
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "EventSourcingService_BidPlace");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, context.Message.UserLogin);
            messageObject.GetType().GetProperty("ItemId").SetValue(messageObject, context.Message.AuctionId);


            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });
            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }

}