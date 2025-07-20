using System.Reflection;
using System.Text.Json;
using Common.Contracts.Communication;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using CommunicationService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CommunicationService.Consumers;

public class CommunicationConsumer : IConsumer<DataForProcessingServicesList<CommunicationItem>>
{
    private readonly CommunicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public CommunicationConsumer(CommunicationDbContext dbContext, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<CommunicationItem>> context)
    {
        var correlationId = context.Message.CorrelationId;
        try
        {
            foreach (var item in context.Message.DataObjects)
            {
                var typedItem = JsonSerializer.Deserialize<CommunicationItem>(item.Data);
                typedItem.CorrelationId = correlationId;
                switch (item.CRUD)
                {
                    case CRUD.Create:
                        typedItem.Commited = false;
                        await _dbContext.Communications.AddAsync(typedItem);
                        break;
                    case CRUD.Delete:
                        //удаляем запись
                        var delItem = await _dbContext.Communications.FirstOrDefaultAsync(p =>
                            p.ItemId == typedItem.ItemId && p.Commited);
                        if (delItem == null)
                        {
                            throw new Exception($"Запись для удаления не найдена, ItemId - {typedItem.ItemId}");
                        }
                        delItem.CorrelationId = correlationId;
                        _dbContext.Communications.Update(delItem);
                        break;
                    case CRUD.Update:
                        var item2 = await _dbContext.Communications.FirstOrDefaultAsync(p =>
                            p.ItemId == typedItem.ItemId && p.Commited);
                        if (item2 == null)
                        {
                            throw new Exception($"Запись для обновления не найдена, Id - {typedItem.ItemId}");
                        }
                        item2.CorrelationId = correlationId;
                        _dbContext.Communications.Update(item2);
                        typedItem.ItemId = item2.ItemId;
                        typedItem.Commited = false;
                        await _dbContext.Communications.AddAsync(typedItem);
                        break;
                    default:
                        break;
                }
            }
            await _dbContext.SaveChangesAsync();

            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
            await _publishEndpoint.Publish(sendObject);
        }
        catch (Exception e)
        {
            //ошибки, в т.ч. штатные
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetInnerException(e).Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "CommunicationService");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
            messageObject.GetType().GetProperty("ItemId").SetValue(messageObject, null);
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);
            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }
}
