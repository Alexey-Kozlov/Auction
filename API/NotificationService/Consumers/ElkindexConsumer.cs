using System.Reflection;
using System.Text.Json;
using Common.Contracts.ELKSearch;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ElkindexConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public ElkindexConsumer(IHubContext<NotificationHub> hubContext,
        IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<NotifyItem>> context)
    {
        var data = context.Message.Props.Split(",");
        //параметр 0 - количество проиндексированных записей
        //параметр 1 - флаг олтображать / не отображать
        //параметр 2 - SessionId
        //уведомление при окончании индексации
        await _hubContext.Clients.Group(data[2]).SendAsync("ElkIndex",
            new { show = Boolean.Parse(data[1]), message = $"Проиндексировано - {data[0]} записей" });

    }
}
