using Common.Contracts;
using MassTransit;
using MetricsService.Metrics;

namespace MetricsService.Consumers;

public class AuctionDeletingMetricsConsumer : IConsumer<AuctionDeletingElk>
{
    private readonly AuctionMetrics _metrics;

    public AuctionDeletingMetricsConsumer(AuctionMetrics metrics)
    {
        _metrics = metrics;
    }
    public async Task Consume(ConsumeContext<AuctionDeletingElk> context)
    {
        //только получаем сообщения об удалении аукциона, ничего не посылаем.
        //таким образом если будет здесь какая-то ошибка - не будет оказано влияние на остальной функционал
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - удален аукцион '{context.Message.Id}'" +
        $", автор - {context.Message.AuctionAuthor}");

        _metrics.DeleteAuction();
        await Task.FromResult<string>(null);

    }
}
