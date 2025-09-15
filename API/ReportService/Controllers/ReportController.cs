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
    private readonly Diagrams _diagrams;

    public ReportController(NotificationList notificationList, AuctionList auctionList, Diagrams diagrams)
    {
        _notificationList = notificationList;
        _auctionList = auctionList;
        _diagrams = diagrams;
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

    [HttpPost("diagrams")]
    public async Task<string> Diagrams([FromBody] ParamItem[] param)
    {
        return await _diagrams.GetDiagrams(param);
    }
}