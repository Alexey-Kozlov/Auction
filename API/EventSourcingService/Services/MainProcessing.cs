using System.Reflection;
using Common.Contracts.EventSourcing;
using MassTransit;

namespace EventSourcingService.Services;

public class MainProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly InsertItemToEventSourcing _insertItemToEventSourcing;
    private readonly IConfiguration _configuration;

    public MainProcessing(IPublishEndpoint publishEndpoint, InsertItemToEventSourcing insertItemToEventSourcing,
        IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _insertItemToEventSourcing = insertItemToEventSourcing;
        _configuration = configuration;
    }
    public async Task Processing(ConsumeContext<ESContract> context)
    {
        //записали в ES запись об обновлении даты окончания аукциона
        await _insertItemToEventSourcing.Processing(context.Message);
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
        await _publishEndpoint.Publish(sendObject);
    }
}