using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using MassTransit;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionUpdate;

public class UpdatingAuctionActivity : IStateMachineActivity<UpdateAuctionState, RequestAuctionUpdate>
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    private readonly ILogger<UpdatingAuctionActivity> _logger;
    public UpdatingAuctionActivity(ITopicProducer<BaseStateContract> topicProducer,
        ILogger<UpdatingAuctionActivity> logger)
    {
        _topicProducer = topicProducer;
        _logger = logger;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<UpdateAuctionState, RequestAuctionUpdate> context, IBehavior<UpdateAuctionState, RequestAuctionUpdate> next)
    {
        _logger.LogInformation($"{DateTime.Now} Команда на создание записи в EventSourcing - команда UpdateAuction");
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var message = new UpdateAuctionStateContract();
        message.Data = JsonSerializer.Serialize(context.Saga, context.Saga.GetType(), options);
        message.Type = nameof(UpdateAuctionStateContract);
        message.CorrelationId = context.Saga.CorrelationId;
        await _topicProducer.Produce(message);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<UpdateAuctionState, RequestAuctionUpdate, TException> context, IBehavior<UpdateAuctionState, RequestAuctionUpdate> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update2");
    }
}