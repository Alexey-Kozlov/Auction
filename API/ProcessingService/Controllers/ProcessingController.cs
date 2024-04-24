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
    public async Task<ActionResult<ProcessingState>> GetStatusAsync(Guid correlationId)
    {
        var response = await _processingClient.GetResponse<ProcessingState>(
            new GetProcessingBidState(correlationId));

        return Ok(response.Message);
    }

    [Authorize]
    [HttpPost("placebid")]
    public async Task<ActionResult> PlaceBid([FromBody] PlaceBidDTO par)
    {
        var bid = new RequestProcessingBidStart(par.auctionId, User.Identity.Name, par.amount, par.correlationId);

        await _publishEndpoint.Publish(bid);

        return Accepted();
    }
}