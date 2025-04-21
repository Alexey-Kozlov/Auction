using System.Linq.Expressions;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using ReportService.Services;
using ReportService.DTO;
using Serialize.Linq.Serializers;
using System.Runtime.Serialization;
using Common.Utils.Extentions;
using Common.Contracts;
using Common.Contracts.Report;

namespace ReportService.Reports;

[KnownType(typeof(List<Guid>))]
public class AuctionList
{
    private readonly IServiceProvider _services;
    public AuctionList(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<string> GetAuctionItems(ParamItem[] param)
    {
        var serializer = new ExpressionSerializer(new JsonSerializer())
        {
            AutoAddKnownTypesAsListTypes = true
        };
        using var scope = _services.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<HttpClientService>();

        var bidSelector = param.FirstOrDefault(p => p.Id == "Bids").Value;
        var bidJson = System.Text.Json.JsonSerializer.Deserialize<SelectJson[]>(bidSelector);
        var bidValue = bidJson.FirstOrDefault(p => p.Default).Value;
        //получаем список аукционов для заданного автора аукциона (или для всех, если никто не указан)
        var auctionTask = Task.Run(() =>
        {
            var par = param.FirstOrDefault(p => p.Id == "Seller").Value;
            //если указан логин пользователчя - автора аукционов для отчета
            Expression<Func<AuctionItem, bool>> auctionExp = item => item.Seller.Contains(par);
            //если не указан автор аукциона - сбрасываем фильтр          
            if (string.IsNullOrEmpty(par))
            {
                auctionExp = item => true;
            }

            switch (bidValue)
            {
                case "NoBids":
                    //добавляем дополнительное условие - без ставок
                    Expression<Func<AuctionItem, bool>> noBid = item => item.CurrentHighBid == 0;
                    auctionExp = auctionExp.CombineWithAndAlso(noBid);
                    break;
                case "Bids":
                    //добавляем дополнительное условие - со ставками
                    Expression<Func<AuctionItem, bool>> withBid = item => item.CurrentHighBid > 0;
                    auctionExp = auctionExp.CombineWithAndAlso(withBid);
                    break;
                case "All":
                    break;
            }
            var auctionExp_text = serializer.SerializeText(auctionExp);
            return httpClient.GetAuctionItems(auctionExp_text);
        });
        var auctions = await auctionTask;


        // если был указан параметр "Со ставками" или "Все" - делаем дополнительный запрос к 
        // микросервису ставок для получения - кто ставил и размера ставок
        if (bidValue != "NoBids")
        {
            //запрос фильтрации по списку id-ников, ids - список id-ников типа GUID
            var ids = auctions.Result.Select(p => p.AuctionId).ToList();
            var bidTask = Task.Run(() =>
            {
                var filterField = "AuctionId";
                var eParam = Expression.Parameter(typeof(BidItem), "e");
                var method = ids.GetType().GetMethod("Contains");
                var call = Expression.Call(Expression.Constant(ids), method, Expression.Property(eParam, filterField));
                var bidExp = Expression.Lambda<Func<BidItem, bool>>(call, eParam);
                var bidExp_text = serializer.SerializeText(bidExp);
                return httpClient.GetBidItems(bidExp_text);
            });
            var bids = await bidTask;

            var resultWithBids = auctions.Result.LeftOuterJoin(
                bids.Result,
                leftKey => leftKey.AuctionId,
                rightKey => rightKey.AuctionId,
                (auction, bid) => new
                {
                    AuctionId = auction.AuctionId,
                    Seller = auction.Seller,
                    Bidder = bid == null ? "" : bid.Bidder,
                    Amount = bid == null ? 0 : bid.Amount,
                    Title = auction.Title,
                    StartDate = auction.CreateAt,
                    EndDate = auction.AuctionEnd
                }
            ).OrderBy(p => p.Seller).ThenByDescending(p => p.StartDate).ThenByDescending(p => p.Amount);
            return System.Text.Json.JsonSerializer.Serialize(resultWithBids, resultWithBids.GetType());
        }

        var resultAuctions = auctions.Result.Select(p => new
        {
            AuctionId = p.AuctionId,
            Seller = p.Seller,
            Title = p.Title,
            StartDate = p.CreateAt,
            EndDate = p.AuctionEnd
        }
        ).OrderBy(p => p.Seller).ThenByDescending(p => p.StartDate);
        return System.Text.Json.JsonSerializer.Serialize(resultAuctions, resultAuctions.GetType());




        // var result = from auctions1 in auctions
        //              join bids1 in bids
        //              on auctions1.AuctionId equals bids1.AuctionId
        //              select new
        //              {
        //                  Auction = auctions1.AuctionId,
        //                  User = bids1.Bidder,
        //                  Title = auctions1.Title
        //              };

    }

}

