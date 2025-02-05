using System.Linq.Expressions;
using Common.Contracts.Auction;
using ReportService.Reports;
using Serialize.Linq.Serializers;

namespace ReportService.Services;

public class GetDataService
{
    private readonly AuctionList _auctionList;
    public GetDataService(AuctionList auctionList)
    {
        _auctionList = auctionList;
    }

    public async Task GetAuctionListData()
    {
        var items = await _auctionList.GetAuctionItems();

    }
}