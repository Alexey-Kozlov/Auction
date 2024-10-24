﻿using System.Net;
using System.Security.Claims;
using Common.Utils;
using Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcessingService.DTO;

namespace ProcessingService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProcessingController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProcessingController> _logger;


    public ProcessingController(IPublishEndpoint publishEndpoint, ILogger<ProcessingController> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

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

    [Authorize(Roles = "Admin")]
    [HttpPost("SetSnapShotDb")]
    public async Task SetSnapShotDb(SessionDTO param)
    {
        //зафиксировать полное состояние БД в EventSourcing
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _publishEndpoint.Publish(new SendAllItems<SendToSetSnapShot>(userLogin, param.SessionId, Guid.NewGuid(), DateTime.UtcNow));
        _logger.LogInformation($"Послан запрос на инициализацию начального состояния");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("elkindex")]
    public async Task ElkIndex(SessionDTO param)
    {
        //Выполняем реиндексацию ELK
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _publishEndpoint.Publish(new SendAllItems<SendToReindexingElk>(userLogin, param.SessionId, Guid.NewGuid(), DateTime.UtcNow));
        _logger.LogInformation($"Послан запрос на переиндексацию ELK");
    }
}