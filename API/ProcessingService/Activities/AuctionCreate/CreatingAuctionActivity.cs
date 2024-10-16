using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.CreateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionCreate;

public class CreatingAuctionActivity : IStateMachineActivity<CreateAuctionState, RequestAuctionCreate>
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    private readonly ILogger<CreatingAuctionActivity> _logger;
    public CreatingAuctionActivity(ITopicProducer<BaseStateContract> topicProducer,
        ILogger<CreatingAuctionActivity> logger)
    {
        _topicProducer = topicProducer;
        _logger = logger;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<CreateAuctionState, RequestAuctionCreate> context, IBehavior<CreateAuctionState, RequestAuctionCreate> next)
    {
        _logger.LogInformation($"{DateTime.Now} Команда на создание записи в EventSourcing - команда CreateAuction");
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var message = new CreateAuctionStateContract();
        message.CorrelationId = context.Saga.CorrelationId;
        message.Type = nameof(CreateAuctionStateContract);
        message.Data = JsonSerializer.Serialize(context.Saga, context.Saga.GetType(), options);
        await _topicProducer.Produce(message);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateAuctionState, RequestAuctionCreate, TException> context, IBehavior<CreateAuctionState, RequestAuctionCreate> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create");
    }
}