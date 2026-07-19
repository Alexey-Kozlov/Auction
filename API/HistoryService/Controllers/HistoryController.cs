using Common.Contracts;
using Common.Contracts.EventSourcing;
using HistoryService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HistoryService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly GrpcSourcingClient _client;
    public HistoryController(GrpcSourcingClient client)
    {
        _client = client;
    }

    //получение истории для аукциона
    [Authorize(Roles = "Admin")]
    [HttpGet("{auctionId:guid}")]
    public async Task<ApiResponse<List<EventsLog>>> GetAuctionHistory([FromRoute] Guid auctionId)
    {
        return new ApiResponse<List<EventsLog>>
        {
            IsSuccess = true,
            StatusCode = System.Net.HttpStatusCode.OK,
            Result = await _client.GetHistoryItems(auctionId)
        };
    }
}
