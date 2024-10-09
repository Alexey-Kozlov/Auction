using System.Net;
using System.Security.Claims;
using Common.Utils;
using Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcessingService.DTO;

namespace ProcessingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessingController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    //private readonly IRequestClient<GetProcessingBidState> _processingClient;

    public ProcessingController(IPublishEndpoint publishEndpoint)
    //IRequestClient<GetProcessingBidState> processingClient)
    {
        _publishEndpoint = publishEndpoint;
        //_processingClient = processingClient;
    }

    // [HttpGet("status/{correlationId}")]
    // public async Task<ApiResponse<CreateBidState>> GetStatusAsync(Guid correlationId)
    // {
    //     var response = await _processingClient.GetResponse<CreateBidState>(
    //         new GetProcessingBidState(correlationId));

    //     return new ApiResponse<CreateBidState>
    //     {
    //         StatusCode = HttpStatusCode.OK,
    //         IsSuccess = true,
    //         Result = response.Message
    //     };
    // }

    [Authorize]
    [HttpPost("placebid")]
    public async Task<ApiResponse<object>> PlaceBid([FromBody] PlaceBidDTO par)
    {
        var bid = new RequestBidPlace(par.Id, User.Identity.Name, par.Amount, par.CorrelationId);

        await _publishEndpoint.Publish(bid);

        return new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.Accepted,
            IsSuccess = true,
            Result = { }
        };
    }
}