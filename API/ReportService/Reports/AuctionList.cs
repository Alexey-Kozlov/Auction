using System.Linq.Expressions;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Utils.Extentions;
using ReportService.Services;
using Serialize.Linq.Serializers;

namespace ReportService.Reports;

public class AuctionList
{
    private readonly IServiceProvider _services;
    public AuctionList(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<List<AuctionItem>> GetAuctionItems()
    {
        var serializer = new ExpressionSerializer(new JsonSerializer());
        using var scope = _services.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<HttpClientService>();

        var auctionTask = Task.Run(() =>
        {
            Expression<Func<AuctionItem, bool>> auctionExp = item => item.Seller == "admin";
            var auctionExp_text = serializer.SerializeText(auctionExp);
            return httpClient.GetAuctionItems(auctionExp_text);
        });

        var bidTask = Task.Run(() =>
        {
            Expression<Func<BidItem, bool>> bidExp = item => item.Bidder == "alice";
            var bidExp_text = serializer.SerializeText(bidExp);
            return httpClient.GetBidItems(bidExp_text);
        });


        await Task.WhenAll(auctionTask, bidTask);
        var auctions = auctionTask.Result;
        var bids = bidTask.Result;

        var result = from auctions1 in auctions
                     join bids1 in bids
                     on auctions1.AuctionId equals bids1.AuctionId
                     select new
                     {
                         Auction = auctions1.AuctionId,
                         User = bids1.Bidder,
                         Title = auctions1.Title
                     };

        var result2 = auctions.LeftOuterJoin(
            bids,
            leftKey => leftKey.AuctionId,
            rightKey => rightKey.AuctionId,
            (leftKey, rightKey) => new
            {
                AuctionId = leftKey.AuctionId,
                User = rightKey == null ? "" : rightKey.Bidder,
                Title = leftKey.Title
            }
        );

        return null;
    }
}

