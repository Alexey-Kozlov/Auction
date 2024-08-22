using Common.Contracts;
using MassTransit;
using MetricsService.Metrics;

namespace MetricsService.Consumers;

public class AuctionCreatingMetricsConsumer : IConsumer<AuctionCreatingElk>
{
    private readonly AuctionMetrics _metrics;

    public AuctionCreatingMetricsConsumer(AuctionMetrics metrics)
    {
        _metrics = metrics;
    }
    public async Task Consume(ConsumeContext<AuctionCreatingElk> context)
    {
        //только получаем сообщения о создании аукциона, ничего не посылаем.
        //таким образом если будет здесь какая-то ошибка - не будет оказано влияние на остальной функционал
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - создан новый аукцион '{context.Message.Title}'" +
        $", автор - {context.Message.AuctionAuthor}");

        _metrics.AddAuction();
        await Task.FromResult<string>(null);

    }
}
