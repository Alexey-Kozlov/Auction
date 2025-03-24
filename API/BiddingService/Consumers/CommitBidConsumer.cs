using System.Reflection;
using BiddingService.Data;
using Common.Contracts.Bid;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Consumers;

public class CommitBidConsumer : IConsumer<BidCommit>
{
    private readonly BidDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public CommitBidConsumer(BidDbContext dbContext, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<BidCommit> context)
    {
        _dbContext.ChangeTracker.Clear();
        var correlationId = context.Message.CorrelationId;
        try
        {
            foreach (var item in await _dbContext.Bids.Where(p =>
                p.CorrelationId == context.Message.CorrelationId).ToListAsync())
            {
                if (context.Message.Commited)
                {
                    //подтверждение транзакции
                    if (item.Commited)
                    {
                        //удаляем старые записи
                        _dbContext.Bids.Remove(item);
                    }
                    else
                    {
                        //подтверждаем новые записи
                        item.Commited = true;
                    }
                }
                else
                {
                    //откат транзакции
                    if (!item.Commited)
                    {
                        //удаляем новые записи
                        _dbContext.Bids.Remove(item);
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            var errorItem = new NotificationServiceError
            {
                CorrelationId = context.Message.CorrelationId,
                Message = e.Message,
                ExceptionMessage = e.Source + "," + e.StackTrace,
                ServiceName = "BiddingService",
                UserLogin = "",
                CallBackType = "",
                IsError = true
            };
            await _publishEndpoint.Publish(errorItem);
        }
    }
}
