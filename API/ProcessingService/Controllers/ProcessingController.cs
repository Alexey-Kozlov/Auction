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
    private readonly SplitImages _splitImages;

    public ProcessingController(IPublishEndpoint publishEndpoint, SplitImages splitImages)
    {
        _publishEndpoint = publishEndpoint;
        _splitImages = splitImages;
    }

    [HttpPost("placebid")]
    public async Task<ApiResponse<object>> PlaceBid([FromBody] PlaceBidDTO par)
    {
        var bid = new RequestBidPlace(par.AuctionId, User.Identity.Name, par.Amount,
            "page", Guid.NewGuid());

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
            ItemId = Guid.NewGuid(),
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
        await _splitImages.ProcessImage(auction);

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
            ItemId = par.ItemId,
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
        var reqAuctionDelete = new RequestAuctionDelete(auctionAuthor, par.ItemId, Guid.NewGuid());

        await _publishEndpoint.Publish(reqAuctionDelete);

        return new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.Accepted,
            IsSuccess = true,
            Result = { }
        };
    }

    [HttpPost("FinanceCreate")]
    public async Task<ApiResponse<object>> FinanceCreate(FinanceAddCreditDTO param)
    {
        //Добавление денег на счет
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _publishEndpoint.Publish(new RequestCreateFinance(param.Amount, userLogin, Guid.NewGuid()));
        return new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.Accepted,
            IsSuccess = true,
            Result = { }
        };
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("elkindex")]
    public async Task ElkIndex()
    {
        //Выполняем реиндексацию ELK
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _publishEndpoint.Publish(new RequestElkIndex
        {
            CorrelationId = Guid.NewGuid(),
            UserLogin = userLogin,
            CallBackType = "",
            ShowMessages = true
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("SetSnapShot")]
    public async Task SetSnapShot()
    {
        //зафиксировать полное состояние БД в EventSourcing - делаем SnapShot
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _publishEndpoint.Publish(new RequestSetSnapShot
        {
            UserLogin = userLogin,
            CorrelationId = Guid.NewGuid()
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("RestoreSnapShot")]
    public async Task RestoreSnapShot(RestoreSnapShotDTO param)
    {
        //Выполняем восстановление БД на указанную дату
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        //корректируем время - приходит по Гринвичу, прибавляем 3 часа - для Московского
        await _publishEndpoint.Publish(new RequestRestoreItems
        {
            RestoreDate = param.RestoreDate.AddHours(3),
            UserLogin = userLogin,
            CorrelationId = Guid.NewGuid(),
            ResetLog = param.ResetLog
        });
    }

    [HttpPost("EditNotification")]
    public async Task<ApiResponse<object>> EditNotification([FromBody] EditNotificationDTO notifyUserDTO)
    {
        //Включение / отключение уведомления для данного пользователя для данного аукциона
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _publishEndpoint.Publish(new RequestEditNotification
        {
            ItemId = notifyUserDTO.ItemId,
            UserLogin = userLogin,
            Enable = notifyUserDTO.Enable,
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
    public async Task ResetImageCache()
    {
        //Выполняем сброс кеша изобюражений
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _publishEndpoint.Publish(new ResetImageCache { UserLogin = userLogin });
    }

    [HttpPost("setuserscurrentpage")]
    public void SetUserCurrentPage([FromBody] string fake)
    {
        //заглушка, смысл - на этот ендпойнт приходит служебный запрос по записи текущей страницы 
        // пользователя, сама запись осуществляется в GatewayService, где обрабатываются логи
    }
}