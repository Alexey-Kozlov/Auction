using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using MassTransit;
using ProcessingService.StateMachines.FinishAuctionStateMachine;

namespace ProcessingService.Activities.AuctionFinish;

public class FinishingAuctionActivity : IStateMachineActivity<FinishAuctionState, RequestAuctionFinish>
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    private readonly ILogger<FinishingAuctionActivity> _logger;
    public FinishingAuctionActivity(ITopicProducer<BaseStateContract> topicProducer,
        ILogger<FinishingAuctionActivity> logger)
    {
        _topicProducer = topicProducer;
        _logger = logger;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<FinishAuctionState, RequestAuctionFinish> context, IBehavior<FinishAuctionState, RequestAuctionFinish> next)
    {
        _logger.LogInformation($"{DateTime.Now} Команда на создание записи в EventSourcing - команда FinishAuction");
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var message = new FinishAuctionStateContract();
        message.Data = JsonSerializer.Serialize(context.Saga, context.Saga.GetType(), options);
        message.Type = nameof(FinishAuctionStateContract);
        message.CorrelationId = context.Saga.CorrelationId;
        await _topicProducer.Produce(message);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<FinishAuctionState, RequestAuctionFinish, TException> context, IBehavior<FinishAuctionState, RequestAuctionFinish> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-finish2");
    }
}