using System.Reflection;
using System.Text.Json;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;

namespace NotificationService.Consumers;

public class EditNotificationConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public EditNotificationConsumer(NotificationDbContext dbContext, IPublishEndpoint publishEndpoint,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<NotifyItem>> context)
    {
        var correlationId = context.Message.CorrelationId;
        try
        {
            foreach (var item in context.Message.DataObjects)
            {
                //уведомления при создании / удалении уведомления
                var typedItem = JsonSerializer.Deserialize<NotifyItem>(item.Data);
                typedItem.CorrelationId = correlationId;
                switch (item.CRUD)
                {
                    case CRUD.Create:
                        typedItem.Commited = false;
                        await _dbContext.NotifyItems.AddAsync(typedItem);
                        break;
                    case CRUD.Delete:
                        //удаляем запись
                        var delItem = await _dbContext.NotifyItems.Where(p =>
                            p.ItemId == typedItem.ItemId && p.Commited &&
                            p.UserLogin == typedItem.UserLogin).FirstOrDefaultAsync();
                        if (delItem == null)
                        {
                            var errorString = $"Запись для удаления - {typedItem.ItemId}";
                            errorString += $" для пользователя {typedItem.UserLogin} не найдена";
                            throw new Exception(errorString);
                        }
                        delItem.CorrelationId = correlationId;
                        _dbContext.NotifyItems.Update(delItem);
                        break;
                }
                await _dbContext.SaveChangesAsync();
                var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
                sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
                await _publishEndpoint.Publish(sendObject);
            }
        }
        catch (Exception e)
        {
            //ошибки прочие
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetInnerException(e).Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "NotificationService_Commit");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
            messageObject.GetType().GetProperty("AuctionId").SetValue(messageObject, null);
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);

            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }

    }
}
