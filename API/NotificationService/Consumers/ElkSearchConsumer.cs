using System.Reflection;
using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;


namespace NotificationService.Consumers;

public class ElkSearchConsumer : IConsumer<DataForProcessingServicesList<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public ElkSearchConsumer(IHubContext<NotificationHub> hubContext,
        IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>> context)
    {
        foreach (var item in context.Message.DataObjects)
        {
            //уведомление при окончании поиска
            var typedItem = JsonSerializer.Deserialize<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>(item.Data);
            await _hubContext.Clients.Group(context.Message.Props).SendAsync("ElkSearch", typedItem.Result);
        }
    }
}
