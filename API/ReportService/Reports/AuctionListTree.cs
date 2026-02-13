using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Report;
using Common.Utils.Extentions;
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

    public Task<AuctionTreeItem[]> GetAuctionTreeItems(ParamItemDTO[] param)
    {
        var sellerPar = param.FirstOrDefault(p => p.Id == "Seller").Value;
        var searchText = param.FirstOrDefault(p => p.Id == "SearchText").Value;
        var query = new SqlQuery();
        List<AuctionItem> auctionList;
        List<BidItem> bidList;
        //получаем список аукционов для заданного автора аукциона (или для всех, если никто не указан)
        query.Text = "select * from \"SearchItems\" where true";

        if (!string.IsNullOrEmpty(searchText))
        {
            query.Text += " and (\"Title\" ilike {0} or \"Properties\" ilike {0} or \"Description\" ilike {0})";
            query.Parameters.Add("%" + searchText + "%");
        }

        if (!string.IsNullOrEmpty(sellerPar))
        {
            var parNumber = string.IsNullOrEmpty(searchText) ? "0" : "1";
            query.Text += " and \"Seller\" ilike {" + parNumber + "}";
            query.Parameters.Add("%" + sellerPar + "%");
        }

        query.Text += " order by \"ItemId\" limit 1000";
        auctionList = _client.GetAuctionReportItems(JsonSerializer.Serialize(query))
            .GetAwaiter().GetResult().Result;

        if (auctionList.Count() == 0) return Task.FromResult<AuctionTreeItem[]>(null);

        query.Text = "select * from \"BidItems\" where true";
        // если был параметр фильтрации - отбираем ставки по возвращенным id-никам, типа GUID
        // если не было фильтрации по автору - отменяем фильтрацию по полученным id-никам - нет смысла
        if (!string.IsNullOrEmpty(sellerPar))
        {
            query.Text += " and \"AuctionId\" in (";
            query.Text += string.Join(',', auctionList.Select(p => "'" + p.ItemId + "'")) + ")";
        }
        query.Text += " limit 3000";
        query.Parameters.Clear();
        bidList = _client.GetBidReportItems(JsonSerializer.Serialize(query))
            .GetAwaiter().GetResult().Result;

        //делаем иерархическую структуру - аукцион + его ставки
        var rezult = auctionList.LeftOuterJoin(
        bidList,
        p => p.ItemId,
        p => p.AuctionId,
        (auction, bid) =>
        new AuctionTreeItem
        {
            key = auction.ItemId,
            data = new AuctionTreeItemData
            {
                auctionEnd = auction.AuctionEnd,
                createAt = auction.CreateAt,
                seller = auction.Seller,
                title = auction.Title,
                itemid = auction.ItemId,
            },
            children = bidList.Where(b => b.AuctionId == auction.ItemId).Select(p =>
                    new BidTreeItem
                    {
                        key = p.ItemId,
                        data = new BidTreeItemData
                        {
                            amount = p.Amount,
                            createAt = p.BidTime,
                            bidder = p.Bidder
                        }
                    }).ToList()
        }).DistinctBy(p => p.key);

        return Task.FromResult(rezult.ToArray());
    }
}

