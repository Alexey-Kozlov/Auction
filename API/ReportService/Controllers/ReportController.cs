using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportService.DTO;
using ReportService.Services;

namespace ReportService.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportController : ControllerBase
{
    private readonly GetDataService _reportService;

    public ReportController(GetDataService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost("auctionlist")]
    public async Task<string> AuctionList([FromBody] ParamItem[] param)
    {
        return await _reportService.GetAuctionListData(param);
    }

    [HttpPost("notifylist")]
    public async Task<string> NotificationList([FromBody] ParamItem[] param)
    {
        return await _reportService.GetNotificationListData(param);
    }

}