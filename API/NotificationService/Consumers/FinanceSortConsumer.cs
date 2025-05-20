using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.Finance;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;


namespace NotificationService.Consumers;

public class FinanceSortConsumer : IConsumer<ApiResponse<PagedResult<List<FinanceHistoryItem>>>>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public FinanceSortConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<ApiResponse<PagedResult<List<FinanceHistoryItem>>>> context)
    {
        //пересылка результатов сортировки финансовой истории
        await _hubContext.Clients.Group(context.Message.Result.SessionId)
            .SendAsync("FinanceHistory", context.Message.Result);
    }
}
