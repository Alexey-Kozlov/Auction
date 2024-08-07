using Common.Contracts;
using Common.Utils;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ElkSearchCreatingNotificationConsumer : IConsumer<ElkSearchResponse<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public ElkSearchCreatingNotificationConsumer(IHubContext<NotificationHub> hubContext,
    NotificationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<ElkSearchResponse<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>> context)
    {
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - создан новый запрос поиска '{context.Message.SearchTerm}'");
        await _hubContext.Clients.Group(context.Message.UserLogin).SendAsync("ElkSearch", context.Message.Result);
        await _publishEndpoint.Publish(new ElkSearchResponseCompleted(context.Message.CorrelationId));
    }
}
