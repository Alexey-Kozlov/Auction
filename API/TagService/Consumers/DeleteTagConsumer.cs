using System.Reflection;
using Common.Contracts.Processing;
using Common.Contracts.Tag;
using Common.Utils.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TagService.Data;

namespace TagService.Consumers;

public class DeleteTagConsumer : IConsumer<ModifyTag>
{
    private readonly TagDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public DeleteTagConsumer(IPublishEndpoint publishEndpoint, TagDbContext dbContext,
        IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<ModifyTag> context)
    {
        try
        {
            var item = await _dbContext.TagItems.FirstOrDefaultAsync(p =>
                p.AuctionId == context.Message.AuctionId &&
                p.Tag == context.Message.Tag);
            if (item == null)
            {
                throw new Exception($"Не найден тег для удаления, AuctionId - {context.Message.AuctionId}" +
                $", Тэг - {context.Message.Tag}");
            }
            _dbContext.TagItems.Remove(item);
            await _dbContext.SaveChangesAsync();
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
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
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "TagService_DeleteTagConsumer");
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);

            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }
}
