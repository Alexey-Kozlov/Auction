using System.Reflection;
using Common.Contracts.Processing;
using Common.Contracts.Settings;
using Common.Utils.Logging;
using Common.Utils.Settings;
using MassTransit;

namespace CommunicationService.Consumers;

public class SetAdminModeConsumer : IConsumer<SetAdminMode>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly IsAdminModeService _isAdminModeService;

    public SetAdminModeConsumer(IPublishEndpoint publishEndpoint, IConfiguration configuration,
        IsAdminModeService isAdminModeService)
    {
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _isAdminModeService = isAdminModeService;
    }
    public async Task Consume(ConsumeContext<SetAdminMode> context)
    {
        await Task.Run(() => _isAdminModeService.SetAdminMode(context.Message.AdminMode));
    }
}
