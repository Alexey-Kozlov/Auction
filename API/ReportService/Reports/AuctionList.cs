using System.Linq.Expressions;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using ReportService.Services;
using ReportService.DTO;
using Serialize.Linq.Serializers;
using System.Runtime.Serialization;
using Common.Utils.Extentions;

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

        //получаем список аукционов для заданного автора аукциона (или для всех, если никто не указан)
        var auctionTask = Task.Run(() =>
        {
            var par = param.FirstOrDefault(p => p.Id == "Seller").Value;
            Expression<Func<AuctionItem, bool>> auctionExp = item => item.Seller.Contains(par);
            if (string.IsNullOrEmpty(par))
            {
                auctionExp = item => true;
            }

            var auctionExp_text = serializer.SerializeText(auctionExp);
            return httpClient.GetAuctionItems(auctionExp_text);
        });
        var auctions = await auctionTask;

        //если был указан параметр "Указывать ставки по лоту" - делаем дополнительный запрос к микросервису ставок
        var showBids = param.FirstOrDefault(p => p.Id == "ShowBids").Value;
        if (Boolean.Parse(showBids))
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

            var result = auctions.Result.LeftOuterJoin(
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
            return System.Text.Json.JsonSerializer.Serialize(result, result.GetType());
        }

        var result1 = auctions.Result.Select(p => new
        {
            AuctionId = p.AuctionId,
            Seller = p.Seller,
            Title = p.Title,
            StartDate = p.CreateAt,
            EndDate = p.AuctionEnd
        }
        ).OrderBy(p => p.Seller).ThenByDescending(p => p.StartDate);
        return System.Text.Json.JsonSerializer.Serialize(result1, result1.GetType());




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
