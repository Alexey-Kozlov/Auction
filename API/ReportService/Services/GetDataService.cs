using ReportService.DTO;
using ReportService.Reports;

namespace ReportService.Services;

public class GetDataService
{
    private readonly AuctionList _auctionList;
    private readonly NotificationList _notificationList;
    public GetDataService(AuctionList auctionList, NotificationList notificationList)
    {
        _auctionList = auctionList;
        _notificationList = notificationList;
    }

    public async Task<string> GetAuctionListData(ParamItem[] param)
    {
        return await _auctionList.GetAuctionItems(param);
    }

    public async Task<string> GetNotificationListData(ParamItem[] param)
    {
        return await _notificationList.GetNotificationItems(param);
    }
}