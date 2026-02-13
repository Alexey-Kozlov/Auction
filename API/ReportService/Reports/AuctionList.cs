using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Report;
using Common.Utils.Extentions;
using ReportService.DTO;
using ReportService.DTO.ResultDTO;
using ReportService.Services;

namespace ReportService.Reports;

public class AuctionList
{
    private readonly GrpcReportsClient _client;
    public AuctionList(GrpcReportsClient client)
    {
        _client = client;
    }

    public Task<AuctionTreeItemsDTO[]> GetAuctionItems(ParamItemDTO[] param)
    {
        var sellerPar = param.FirstOrDefault(p => p.Id == "Seller").Value;
        var bidsPar = param.FirstOrDefault(p => p.Id == "Bids").Value;
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

        //дополнительная фильтрация по ставкам
        switch (bidsPar)
        {
            case "NoBids":
                //добавляем дополнительное условие - без ставок
                query.Text += " and  \"CurrentHighBid\"=0";
                break;
            case "Bids":
                //добавляем дополнительное условие - со ставками
                query.Text += " and  \"CurrentHighBid\">0";
                break;
            case "All":
                break;
        }
        query.Text += " order by \"ItemId\" limit 3000";
        auctionList = _client.GetAuctionReportItems(JsonSerializer.Serialize(query))
            .GetAwaiter().GetResult().Result;

        if (auctionList.Count() == 0) return Task.FromResult<AuctionTreeItemsDTO[]>(null);
        // если был указан параметр "Со ставками" или "Все" - делаем дополнительный запрос к 
        // микросервису ставок для получения - кто ставил и размера ставок
        if (bidsPar != "NoBids")
        {
            //запрос фильтрации по списку id-ников, ids - список id-ников типа GUID
            query.Text = "select * from \"BidItems\" where true";
            if (!string.IsNullOrEmpty(sellerPar))
            {
                query.Text += " and \"AuctionId\" in (";
                query.Text += string.Join(',', auctionList.Select(p => "'" + p.ItemId + "'")) + ")";
            }
            query.Text += " limit 3000";
            query.Parameters.Clear();
            bidList = _client.GetBidReportItems(JsonSerializer.Serialize(query))
                .GetAwaiter().GetResult().Result;

            var resultWithBids = auctionList.LeftOuterJoin(
                bidList,
                leftKey => leftKey.ItemId,
                rightKey => rightKey.AuctionId,
                (auction, bid) => new AuctionTreeItemsDTO
                {
                    ItemId = auction.ItemId,
                    Seller = auction.Seller,
                    Bidder = bid == null ? "" : bid.Bidder,
                    Amount = bid == null ? 0 : bid.Amount,
                    Title = auction.Title,
                    StartDate = auction.CreateAt,
                    EndDate = auction.AuctionEnd
                }
            ).OrderBy(p => p.Seller).ThenByDescending(p => p.StartDate).ThenByDescending(p => p.Amount);
            return Task.FromResult(resultWithBids.ToArray());
        }

        var resultAuctions = auctionList.Select(p => new AuctionTreeItemsDTO
        {
            ItemId = p.ItemId,
            Seller = p.Seller,
            Title = p.Title,
            StartDate = p.CreateAt,
            EndDate = p.AuctionEnd
        }
        ).OrderBy(p => p.Seller).ThenByDescending(p => p.StartDate);
        return Task.FromResult(resultAuctions.ToArray());
    }
}

