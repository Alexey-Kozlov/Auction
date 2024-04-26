using System.Net;
using Common.Utils;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcessingService.DTO;
using ProcessingService.StateMachines;

namespace ProcessingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessingController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    private readonly IRequestClient<GetProcessingBidState> _processingClient;

    public ProcessingController(IPublishEndpoint publishEndpoint, IRequestClient<GetProcessingBidState> processingClient)
    {
        _publishEndpoint = publishEndpoint;
        _processingClient = processingClient;
    }

    [HttpGet("status/{correlationId}")]
    public async Task<ApiResponse<ProcessingState>> GetStatusAsync(Guid correlationId)
    {
        var response = await _processingClient.GetResponse<ProcessingState>(
            new GetProcessingBidState(correlationId));

        return new ApiResponse<ProcessingState>
        {
            StatusCode = HttpStatusCode.OK,
            IsSuccess = true,
            Result = response.Message
        };
    }

    [Authorize]
    [HttpPost("placebid")]
    public async Task<ApiResponse<object>> PlaceBid([FromBody] PlaceBidDTO par)
    {
        var bid = new RequestProcessingBidStart(par.auctionId, User.Identity.Name, par.amount, par.correlationId);

        await _publishEndpoint.Publish(bid);

        return new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.Accepted,
            IsSuccess = true,
            Result = { }
        };
    }
}