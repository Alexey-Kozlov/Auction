using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportService.DTO;
using ReportService.Reports;

namespace ReportService.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportController : ControllerBase
{
    private readonly NotificationList _notificationList;
    private readonly AuctionList _auctionList;

    public ReportController(NotificationList notificationList, AuctionList auctionList)
    {
        _notificationList = notificationList;
        _auctionList = auctionList;
    }

    [HttpPost("auctionlist")]
    public async Task<string> AuctionList([FromBody] ParamItem[] param)
    {
        return await _auctionList.GetAuctionItems(param);
    }

    [HttpPost("notifylist")]
    public async Task<string> NotificationList([FromBody] ParamItem[] param)
    {
        return await _notificationList.GetNotificationItems(param);
    }

}