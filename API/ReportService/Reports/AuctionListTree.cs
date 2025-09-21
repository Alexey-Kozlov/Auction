using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Report;
using ReportService.DTO;
using ReportService.Services;

namespace ReportService.Reports;

public class AuctionListTree
{
    private readonly GrpcReportsClient _client;
    public AuctionListTree(GrpcReportsClient client)
    {
        _client = client;
    }

    public Task<string> GetAuctionTreeItems(ParamItemDTO[] param)
    {
        var sellerPar = param.FirstOrDefault(p => p.Id == "Seller").Value;
        var query = new SqlQuery();
        List<AuctionItem> auctionList;
        List<BidItem> bidList;
        //получаем список аукционов для заданного автора аукциона (или для всех, если никто не указан)
        query.Text = "select * from \"SearchItems\" where true";
        if (!string.IsNullOrEmpty(sellerPar))
        {
            query.Text += " and \"Seller\" ilike {0}";
            query.Parameters.Add("%" + sellerPar + "%");
        }

        query.Text += " order by \"ItemId\" limit 1000";
        auctionList = _client.GetAuctionReportItems(JsonSerializer.Serialize(query))
            .GetAwaiter().GetResult().Result;

        if (auctionList.Count() == 0) return Task.FromResult("[]");
        // делаем дополнительный запрос к микросервису ставок для получения - кто ставил и размера ставок
        //запрос фильтрации по списку id-ников, ids - список id-ников типа GUID
        query.Text = "select * from \"BidItems\" where \"AuctionId\" in (";
        query.Text += string.Join(',', auctionList.Select(p => "'" + p.ItemId + "'"));
        query.Text += ") limit 3000";
        query.Parameters.Clear();
        bidList = _client.GetBidReportItems(JsonSerializer.Serialize(query))
            .GetAwaiter().GetResult().Result;

        //делаем иерархическую структуру - аукцион + его ставки
        var rezult = new AuctionTreeItem[auctionList.Count];
        var i = 0;
        foreach (var auctionItem in auctionList.OrderBy(p => p.Title))
        {
            rezult[i] = new AuctionTreeItem
            {
                key = auctionItem.ItemId,
                data = new AuctionTreeItemData
                {
                    auctionEnd = auctionItem.AuctionEnd,
                    createAt = auctionItem.CreateAt,
                    seller = auctionItem.Seller,
                    title = auctionItem.Title,
                    itemid = auctionItem.ItemId
                },
                children = bidList.Where(b => b.AuctionId == auctionItem.ItemId).Select(p =>
                    new BidTreeItem
                    {
                        key = p.ItemId,
                        data = new BidTreeItemData
                        {
                            amount = p.Amount,
                            createAt = p.BidTime,
                            bidder = p.Bidder
                        }
                    }).OrderByDescending(p => p.data.amount).ToArray()
            };
            i++;
        }
        return Task.FromResult(JsonSerializer.Serialize(rezult));
    }
}

