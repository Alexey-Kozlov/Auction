using System.Reflection;
using AutoMapper;
using Common.Contracts.Processing;
using Common.Contracts.Tag;
using Common.Utils.Logging;
using MassTransit;
using TagService.Data;

namespace TagService.Consumers;

public class CreateTagConsumer : IConsumer<ModifyTag>
{
    private readonly TagDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public CreateTagConsumer(IPublishEndpoint publishEndpoint, TagDbContext dbContext,
        IConfiguration configuration, IMapper mapper)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<ModifyTag> context)
    {
        try
        {
            var item = _mapper.Map<TagItem>(context.Message);
            await _dbContext.TagItems.AddAsync(item);
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
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetMessage(e));
            messageObject.GetType().GetProperty("ErrorExceptionStack").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorExceptionInputData").SetValue(messageObject, GetErrorMessage.GetExceptionStringData(context.Message));
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "TagService_CreateTagConsumer");


            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }
}
