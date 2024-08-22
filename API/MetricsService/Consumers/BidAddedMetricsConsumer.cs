using Common.Contracts;
using MassTransit;
using MetricsService.Metrics;

namespace MetricsService.Consumers;

public class BidAddedMetricsConsumer : IConsumer<BidPlacing>
{
    private readonly AuctionMetrics _metrics;

    public BidAddedMetricsConsumer(AuctionMetrics metrics)
    {
        _metrics = metrics;
    }
    public async Task Consume(ConsumeContext<BidPlacing> context)
    {
        //только получаем сообщения о создании ставки на аукционе, ничего не посылаем.
        //таким образом если будет здесь какая-то ошибка - не будет оказано влияние на остальной функционал
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - новая ставка на аукционе '{context.Message.Bidder}'" +
        $", автор - {context.Message.Amount}");

        _metrics.IncreaseAuctionBid();
        await Task.FromResult<string>(null);

    }
}
