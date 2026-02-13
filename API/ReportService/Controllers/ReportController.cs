using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportService.DTO;
using ReportService.DTO.ResultDTO;
using ReportService.Reports;

namespace ReportService.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportController : ControllerBase
{
    private readonly NotificationList _notificationList;
    private readonly AuctionList _auctionList;
    private readonly AuctionListTree _auctionListTree;
    private readonly Diagrams _diagrams;
    private readonly Comments _comments;

    public ReportController(NotificationList notificationList, AuctionList auctionList,
        Diagrams diagrams, AuctionListTree auctionListTree, Comments comments)
    {
        _notificationList = notificationList;
        _auctionList = auctionList;
        _diagrams = diagrams;
        _auctionListTree = auctionListTree;
        _comments = comments;
    }

    [HttpPost("auctionlist")]
    public async Task<AuctionTreeItemsDTO[]> AuctionList([FromBody] ParamItemDTO[] param)
    {
        return await _auctionList.GetAuctionItems(param);
    }

    [HttpPost("auctionlisttree")]
    public async Task<AuctionTreeItem[]> AuctionListTree([FromBody] ParamItemDTO[] param)
    {
        return await _auctionListTree.GetAuctionTreeItems(param);
    }

    [HttpPost("notifylist")]
    public async Task<NotificationItemsDTO[]> NotificationList([FromBody] ParamItemDTO[] param)
    {
        return await _notificationList.GetNotificationItems(param);
    }

    [HttpPost("diagrams")]
    public async Task<DiagramItemsDTO[]> Diagrams([FromBody] ParamItemDTO[] param)
    {
        return await _diagrams.GetDiagrams(param);
    }

    [HttpPost("comments")]
    public async Task<AuctionTreeItemCommunication[]> Comments([FromBody] ParamItemDTO[] param)
    {
        return await _comments.GetCommentsItems(param);
    }
}