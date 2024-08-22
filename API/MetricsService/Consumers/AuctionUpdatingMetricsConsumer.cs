using Common.Contracts;
using MassTransit;
using MetricsService.Metrics;

namespace MetricsService.Consumers;

public class AuctionUpdatingMetricsConsumer : IConsumer<AuctionUpdatingElk>
{
    private readonly AuctionMetrics _metrics;

    public AuctionUpdatingMetricsConsumer(AuctionMetrics metrics)
    {
        _metrics = metrics;
    }
    public async Task Consume(ConsumeContext<AuctionUpdatingElk> context)
    {
        //только получаем сообщения об обновлении аукциона, ничего не посылаем.
        //таким образом если будет здесь какая-то ошибка - не будет оказано влияние на остальной функционал
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - обновлен аукцион '{context.Message.Title}'" +
        $", автор - {context.Message.AuctionAuthor}");

        _metrics.UpdateAuction();
        await Task.FromResult<string>(null);

    }
}
