using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        //пример - если пришла ошибка с шины данных - ошибку можно обработать и снова послать какое-то сообщение
        var exception = context.Message.Exceptions.First();
        if (exception.ExceptionType == "System.ArgumentException")
        {
            context.Message.Message.Title = "Белаз";
            await context.Publish(context.Message.Message);
        }
        Console.WriteLine("--> Получение сообщения - Ошибка создания аукциона - " + context.Message.FaultedMessageId);
    }
}
