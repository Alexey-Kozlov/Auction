using System.Reflection;
using Common.Contracts.Finance;
using Common.Contracts.Processing;
using FinanceService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class CommitFinanceConsumer : IConsumer<FinanceCommit>
{
    private readonly FinanceDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public CommitFinanceConsumer(FinanceDbContext dbContext, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<FinanceCommit> context)
    {
        _dbContext.ChangeTracker.Clear();
        var correlationId = context.Message.CorrelationId;
        try
        {
            foreach (var item in await _dbContext.FinanceItems.Where(p =>
                p.CorrelationId == context.Message.CorrelationId).ToListAsync())
            {
                if (context.Message.Commited)
                {
                    //подтверждение транзакции
                    if (item.Commited)
                    {
                        //удаляем старые записи
                        _dbContext.FinanceItems.Remove(item);
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
                        _dbContext.FinanceItems.Remove(item);
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
                ServiceName = "FinanceService",
                UserLogin = "",
                AuctionId = null,
                TraceId = Guid.NewGuid(),
                IsError = true
            };
            await _publishEndpoint.Publish(errorItem);
        }
    }
}
