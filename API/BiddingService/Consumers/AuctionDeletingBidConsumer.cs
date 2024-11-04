using System.Reflection;
using BiddingService.Data;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;


namespace BiddingService.Consumers;

public class AuctionDeletingBidConsumer : IConsumer<ActionMessageList<ComplexAuctionBidItem>>
{
    private readonly BidDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public AuctionDeletingBidConsumer(BidDbContext dbContext, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<ActionMessageList<ComplexAuctionBidItem>> context)
    {
        var correlationId = context.Message.ActionItemsList[0].CorrelationId;
        var auctionBidItem = context.Message.ActionItemsList[0].ActionItem.auctionBidItem;
        var bids = context.Message.ActionItemsList[0].ActionItem.bidItems;
        //удаляем ставки Bid
        _dbContext.Bids.RemoveRange(bids);
        //удаляем AuctionBid
        _dbContext.Auctions.Remove(auctionBidItem);
        await _dbContext.SaveChangesAsync();
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }


}
