﻿using BiddingService.Data;
using BiddingService.Models;
using BiddingService.Services;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Consumers;

public class BidPlacedConsumer : IConsumer<RequestBidPlace>
{
    private readonly BidDbContext _dbContext;
    private readonly GrpcAuctionClient _grpcClient;
    private readonly IPublishEndpoint _publishEndpoint;

    public BidPlacedConsumer(IPublishEndpoint publishEndpoint, BidDbContext dbContext, GrpcAuctionClient grpcClient)
    {
        _dbContext = dbContext;
        _grpcClient = grpcClient;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<RequestBidPlace> context)
    {
        var auction = await _dbContext.Auctions.FirstOrDefaultAsync(p => p.Id == context.Message.AuctionId);
        if (auction == null)
        {
            auction = _grpcClient.GetAuction(context.Message.AuctionId.ToString());
            // if (auction == null) return new ApiResponse<BidDTO>()
            // {
            //     StatusCode = System.Net.HttpStatusCode.BadRequest,
            //     IsSuccess = false,
            //     ErrorMessages = ["Невозможно назначить заявку на этот аукцион - аукцион не найден!"],
            //     Result = null
            // };
        }

        // if (auction.Seller == context.Message.Bidder)
        // {
        //     return new ApiResponse<BidDTO>()
        //     {
        //         StatusCode = System.Net.HttpStatusCode.BadRequest,
        //         IsSuccess = false,
        //         ErrorMessages = ["Невозможно подать предложение для собственного аукциона"],
        //         Result = null
        //     };
        // }

        var bid = new Bid
        {
            Amount = context.Message.Amount,
            AuctionId = context.Message.AuctionId,
            Bidder = context.Message.Bidder
        };

        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Завершено;
        }
        else
        {
            var highBid = await _dbContext.Bids.Where(p => p.AuctionId == context.Message.AuctionId)
                .OrderByDescending(p => p.Amount).FirstOrDefaultAsync();

            if (highBid != null && context.Message.Amount > highBid.Amount || highBid == null)
            {
                bid.BidStatus = BidStatus.Принято;
            }
        }
        await _dbContext.Bids.AddAsync(bid);
        await _dbContext.SaveChangesAsync();

        await _publishEndpoint.Publish(new BidPlaced(
            context.Message.CorrelationId,
            context.Message.AuctionId,
            context.Message.Amount));

        Console.WriteLine("--> Получение сообщения - размещена заявка - " +
                 context.Message.Bidder + ", " + context.Message.Amount);
    }
}
