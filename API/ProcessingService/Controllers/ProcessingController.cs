using System.Net;
using System.Security.Claims;
using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.ELKSearch;
using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcessingService.DTO;
using ProcessingService.Services;

namespace ProcessingService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProcessingController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProcessingController> _logger;
    private readonly SplitImages _splitImages;

    public ProcessingController(IPublishEndpoint publishEndpoint,
        ILogger<ProcessingController> logger, SplitImages splitImages)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _splitImages = splitImages;
    }

    [HttpPost("placebid")]
    public async Task<ApiResponse<object>> PlaceBid([FromBody] PlaceBidDTO par)
    {
        var bid = new RequestBidPlace(par.AuctionId, User.Identity.Name, par.Amount, Guid.NewGuid());

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
        var auction = new RequestAuctionCreate
        {
            AuctionId = Guid.NewGuid(),
            ReservePrice = par.ReservePrice,
            AuctionEnd = par.AuctionEnd,
            Properties = par.Properties,
            Title = par.Title,
            Description = par.Description,
            Image = par.Image,
            UserLogin = auctionAuthor,
            CorrelationId = Guid.NewGuid(),
            UsingImage = par.UsingImage,
            IsImageSplitted = false
        };
        //делим изображения на части (если изображение слишком большое)
        await _splitImages.ProcessImage<RequestAuctionCreate>(auction);

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
        var auction = new RequestAuctionUpdate
        {
            AuctionId = par.AuctionId,
            Title = par.Title,
            Properties = par.Properties,
            Image = par.Image,
            Description = par.Description,
            UserLogin = User.Identity.Name,
            AuctionEnd = par.AuctionEnd,
            CorrelationId = Guid.NewGuid(),
            UsingImage = par.UsingImage,
            IsImageSplitted = false
        };
        //делим изображения на части (если изображение слишком большое)
        await _splitImages.ProcessImage<RequestAuctionUpdate>(auction);
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
        var reqAuctionDelete = new RequestAuctionDelete(Guid.NewGuid(), auctionAuthor, par.AuctionId);

        await _publishEndpoint.Publish(reqAuctionDelete);

        return new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.Accepted,
            IsSuccess = true,
            Result = { }
        };
    }

    [Authorize]
    [HttpPost("FinanceCreate")]
    public async Task<ApiResponse<object>> FinanceCreate(FinanceAddCreditDTO param)
    {
        //Добавление денег на счет
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _publishEndpoint.Publish(new RequestCreateFinance(param.Amount, userLogin, Guid.NewGuid(), param.SessionId));
        return new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.Accepted,
            IsSuccess = true,
            Result = { }
        };
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("elkindex")]
    public async Task ElkIndex(SessionDTO param)
    {
        //Выполняем реиндексацию ELK
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _publishEndpoint.Publish(new RequestElkIndex(userLogin, Guid.NewGuid(), param.SessionId));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("SetSnapShot")]
    public async Task SetSnapShot(SessionDTO param)
    {
        //зафиксировать полное состояние БД в EventSourcing - делаем SnapShot
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _publishEndpoint.Publish(new RequestSetSnapShot
        {
            UserLogin = userLogin,
            CorrelationId = Guid.NewGuid(),
            SessionId = param.SessionId
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("RestoreSnapShot")]
    public async Task RestoreSnapShot(RestoreSnapShotDTO param)
    {
        //Выполняем восстановление БД на указанную дату
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _publishEndpoint.Publish(new RequestRestoreItems
        {
            RestoreDate = param.RestoreDate,
            UserLogin = userLogin,
            CorrelationId = Guid.NewGuid(),
            SessionId = param.SessionId,
            ResetLog = param.ResetLog
        });
    }

    [Authorize]
    [HttpPost("EditNotification")]
    public async Task<ApiResponse<object>> EditNotification([FromBody] EditNotificationDTO notifyUserDTO)
    {
        //Включение / отключение уведомления для данного пользователя для данного аукциона
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _publishEndpoint.Publish(new RequestEditNotification
        {
            AuctionId = notifyUserDTO.AuctionId,
            UserLogin = userLogin,
            Enable = notifyUserDTO.Enable,
            SessionId = notifyUserDTO.SessionId,
            CorrelationId = Guid.NewGuid()
        });
        return new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.Accepted,
            IsSuccess = true,
            Result = { }
        };
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("resetimagecache")]
    public async Task ResetImageCache(SessionDTO param)
    {
        //Выполняем сброс кеша изобюражений
        await _publishEndpoint.Publish(new ResetImageCache { SessionId = param.SessionId });
    }
}