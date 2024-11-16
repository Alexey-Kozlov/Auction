using System.Reflection;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class ESLogCommitProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public ESLogCommitProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task CommitESLog(ConsumeContext<ESContract> context)
    {
        var ret = await _dbContext.commit_operation(context.Message.CorrelationId).FirstOrDefaultAsync();
        if (ret.result)
        {
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
            await _publishEndpoint.Publish(sendObject);
        }
        else
        {
            //ошибка при подтверждении транзакции
            // Fault<BidFinanceGranted> per = new FaultMessage<BidFinanceGranted>(
            //     e.MessageText,
            //     new BidFinanceGranted { CorrelationId = context.CorrelationId }
            // );
            await _publishEndpoint.Publish(null);
        }
    }

}