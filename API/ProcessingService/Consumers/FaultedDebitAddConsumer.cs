using Contracts;
using MassTransit;

namespace ProcessingService.Consumers;

public class FaultedDebitAddConsumer : IConsumer<RequestFinanceDebitAdd>
{

    public FaultedDebitAddConsumer()
    {
    }
    public Task Consume(ConsumeContext<RequestFinanceDebitAdd> context)
    {

        Console.WriteLine("--> Завершение процесса - превышение кредитного лимита, - " +
                 context.Message.Debit + ", " + context.Message.UserLogin);
        return Task.CompletedTask;

    }
}
