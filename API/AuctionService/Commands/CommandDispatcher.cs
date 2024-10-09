using Common.Contracts;

namespace AuctionService.Commands;

public class CommandDispatcher : ICommandDispatcher
{
    private readonly Dictionary<Type, Func<BaseCommand, Task>> _handlers = new();

    public CommandDispatcher(ICommandHandler commandHandler)
    {
        this.RegisterHandler<CreateAuctionCommand>(commandHandler.HandlerAsync);
        this.RegisterHandler<UpdateAuctionCommand>(commandHandler.HandlerAsync);
        this.RegisterHandler<DeleteAuctionCommand>(commandHandler.HandlerAsync);
    }

    public void RegisterHandler<T>(Func<T, Task> handler) where T : BaseCommand
    {
        if (_handlers.ContainsKey(typeof(T)))
        {
            throw new IndexOutOfRangeException($"Невозможно дважды зарегистрировать хендлер {nameof(T)}");
        }
        _handlers.Add(typeof(T), x => handler((T)x));
    }

    public async Task SendAsync(BaseCommand command)
    {
        if (_handlers.TryGetValue(command.GetType(), out Func<BaseCommand, Task> handler))
        {
            await handler(command);
        }
        else
        {
            throw new ArgumentNullException($"Хендлер {nameof(handler)} не зарегестрирован");
        }
    }
}