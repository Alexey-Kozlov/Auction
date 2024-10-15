using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.Activities.AuctionDelete;

public class DeletingAuctionActivity : IStateMachineActivity<DeleteAuctionState, RequestAuctionDelete>
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    private readonly ILogger<DeletingAuctionActivity> _logger;
    public DeletingAuctionActivity(ITopicProducer<BaseStateContract> topicProducer,
        ILogger<DeletingAuctionActivity> logger)
    {
        _topicProducer = topicProducer;
        _logger = logger;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<DeleteAuctionState, RequestAuctionDelete> context, IBehavior<DeleteAuctionState, RequestAuctionDelete> next)
    {
        _logger.LogInformation($"{DateTime.Now} Команда на создание записи в EventSourcing - команда DeleteAuction");
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var message = new DeleteAuctionStateContract();
        message.Data = JsonSerializer.Serialize(context.Saga, context.Saga.GetType(), options);
        message.Type = nameof(DeleteAuctionStateContract);
        message.CorrelationId = context.Saga.CorrelationId;
        await _topicProducer.Produce(message);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteAuctionState, RequestAuctionDelete, TException> context, IBehavior<DeleteAuctionState, RequestAuctionDelete> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-delete2");
    }
}