using Common.Contracts.Settings;
using Common.Utils.Settings;
using MassTransit;

namespace NotificationService.Consumers;

public class SetAdminModeConsumer : IConsumer<SetAdminMode>
{
    private readonly IsAdminModeService _isAdminModeService;

    public SetAdminModeConsumer(IsAdminModeService isAdminModeService)
    {
        _isAdminModeService = isAdminModeService;
    }
    public async Task Consume(ConsumeContext<SetAdminMode> context)
    {
        await Task.Run(() => _isAdminModeService.SetAdminMode(context.Message.AdminMode));
    }
}
