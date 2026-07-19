using System.Text.Json;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using EventSourcingService.Data;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SourcingService;

namespace EventSourcingService.Services;

[Authorize]
public class GrpcSourcingService : GrpcSourcing.GrpcSourcingBase
{
    private readonly EventSourcingDbContext _dbContext;
    public GrpcSourcingService(EventSourcingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // получили от HistoryService запрос для выборки данных по истории аукциона
    public override async Task<GrpcHistoryResponse> GetHistory(GetHistoryRequest request, ServerCallContext context)
    {
        var auctionId = Guid.Parse(request.HistoryRequest);
        var items = await _dbContext.EventsLogs.Where(p => p.AuctionId == auctionId)
        .GroupBy(p => new { p.CorrelationId, p.Command, p.UserLogin })
        .Select(p => new EventsLog
        {
            CorrelationId = p.Key.CorrelationId,
            EntityType = ProcessingServiceMethods.GetCommandName(p.Key.Command),
            CreateAt = p.Max(q => q.CreateAt.AddHours(3)),
            UserLogin = p.Key.UserLogin
        })
        .OrderByDescending(p => p.CreateAt)
        .ToListAsync();

        return new GrpcHistoryResponse
        {
            HistoryResponse = JsonSerializer.Serialize(items)
        };
    }

}
