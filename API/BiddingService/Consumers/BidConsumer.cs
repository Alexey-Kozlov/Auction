using System.Reflection;
using System.Text.Json;
using BiddingService.Data;
using Common.Contracts.Bid;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.EntityFrameworkCore;


namespace BiddingService.Consumers;

public class BidConsumer : IConsumer<DataForProcessingServicesList<BidItem>>
{
    private readonly BidDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public BidConsumer(BidDbContext dbContext, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<BidItem>> context)
    {
        var correlationId = context.Message.CorrelationId;
        var props = !string.IsNullOrEmpty(context.Message.Props) ? context.Message.Props : "";
        if (props == "RestoreSnapShot")
        {

        }
        else
        {
            using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            foreach (var item in context.Message.DataObjects)
            {
                var typedItem = JsonSerializer.Deserialize<BidItem>(item.Data);
                switch (item.CRUD)
                {
                    case CRUD.Create:
                        await _dbContext.Bids.AddAsync(typedItem);
                        break;
                    case CRUD.Delete:
                        //удаляем запись
                        var delItem = await _dbContext.Bids.FindAsync(typedItem.BidId);
                        if (delItem == null)
                        {
                            throw new Exception($"Запись для удаления не найдена");
                        }
                        _dbContext.Bids.Remove(delItem);
                        break;
                    default:
                        break;
                }
            }
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
