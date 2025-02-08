using ReportService.DTO;
using ReportService.Reports;

namespace ReportService.Services;

public class GetDataService
{
    private readonly AuctionList _auctionList;
    public GetDataService(AuctionList auctionList)
    {
        _auctionList = auctionList;
    }

    public async Task<string> GetAuctionListData(ParamItem[] param)
    {
        return await _auctionList.GetAuctionItems(param);
    }
}