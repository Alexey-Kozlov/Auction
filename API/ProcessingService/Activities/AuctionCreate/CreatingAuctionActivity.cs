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
        _logger.LogInformation($"Команда на создание записи в EventSourcing - команда CreateAuction");
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var message = new CreateAuctionStateContract();
        //нужно удалить изображение из объекта - его не будем передавать в Kafka
        //для этого делаем глубокую копию объекта и ее правим
        //var auctionString = JsonSerializer.Serialize(context.Saga, context.Saga.GetType(), options);
        //var auctionObject = JsonSerializer.Deserialize<CreateAuctionState>(auctionString, options);

        var auctionItem = context.Saga;
        var nau = new CreateAuctionState();

        DeepCopyUtil.DeepCopy<CreateAuctionState>(ref auctionItem, ref nau);

        nau.Image = "";

        message.CorrelationId = context.Saga.CorrelationId;
        message.Type = nameof(CreateAuctionStateContract);

        message.Data = JsonSerializer.Serialize(nau, nau.GetType(), options);
        await _topicProducer.Produce(message);
        _logger.LogInformation($"Команда на создание записи в EventSourcing - команда CreateAuction - послано");
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateAuctionState, RequestAuctionCreate, TException> context, IBehavior<CreateAuctionState, RequestAuctionCreate> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}