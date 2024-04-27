using Contracts;
using MassTransit;

namespace ProcessingService.Consumers;

public class FaultedBidPlaceConsumer : IConsumer<RequestBidPlace>
{

    public FaultedBidPlaceConsumer()
    {
    }
    public Task Consume(ConsumeContext<RequestBidPlace> context)
    {

        Console.WriteLine("--> Завершение процесса - ошибка размещения ставки, - " +
                 context.Message.Amount + ", " + context.Message.Bidder);
        return Task.CompletedTask;

    }
}
