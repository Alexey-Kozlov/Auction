using System.Net;
using System.Security.Claims;
using Common.Utils;
using Contracts;
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

    [Authorize]
    [HttpPost("createauction")]
    public async Task<ApiResponse<object>> CreateAuction([FromBody] CreateAuctionDTO par)
    {
        var auctionAuthor = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        var auction = new RequestAuctionCreate(Guid.NewGuid(), par.ReservePrice, par.AuctionEnd,
         par.Properties, par.Title, par.Description, par.Image, auctionAuthor, par.CorrelationId);

        await _publishEndpoint.Publish(auction);

        return new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.Accepted,
            IsSuccess = true,
            Result = { }
        };
    }

    [Authorize]
    [HttpPost("updateauction")]
    public async Task<ApiResponse<object>> UpdateAuction([FromBody] UpdateAuctionDTO par)
    {
        var auction = new RequestAuctionUpdate(par.Id, par.Title, par.Properties, par.Image, par.Description,
        User.Identity.Name, par.AuctionEnd, par.CorrelationId);

        await _publishEndpoint.Publish(auction);

        return new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.Accepted,
            IsSuccess = true,
            Result = { }
        };
    }

    [Authorize]
    [HttpPost("deleteauction")]
    public async Task<ApiResponse<object>> DeleteAuction([FromBody] DeleteAuctionDTO par)
    {
        var auctionAuthor = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        var reqAuctionDelete = new RequestAuctionDelete(par.CorrelationId, auctionAuthor, par.Id);

        await _publishEndpoint.Publish(reqAuctionDelete);

        return new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.Accepted,
            IsSuccess = true,
            Result = { }
        };
    }
}