using System.Reflection;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class CommitSearchConsumer : IConsumer<AuctionCommit>
{
    private readonly SearchDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public CommitSearchConsumer(SearchDbContext dbContext, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<AuctionCommit> context)
    {
        _dbContext.ChangeTracker.Clear();
        var correlationId = context.Message.CorrelationId;
        try
        {
            foreach (var item in await _dbContext.AuctionItems.Where(p =>
                p.CorrelationId == context.Message.CorrelationId).ToListAsync())
            {
                if (context.Message.Commited)
                {
                    //подтверждение транзакции
                    if (item.Commited)
                    {
                        //удаляем старые записи
                        _dbContext.AuctionItems.Remove(item);
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
                        _dbContext.AuctionItems.Remove(item);
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
                ServiceName = "SearchService",
                UserLogin = "",
                TraceId = Guid.NewGuid(),
                IsError = true
            };
            await _publishEndpoint.Publish(errorItem);
        }
    }
}

