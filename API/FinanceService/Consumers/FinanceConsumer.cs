using System.Reflection;
using System.Text.Json;
using Common.Contracts.Finance;
using Common.Contracts.Processing;
using FinanceService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class FinanceConsumer : IConsumer<DataForProcessingServicesList<FinanceItem>>
{
    private readonly FinanceDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public FinanceConsumer(FinanceDbContext dbContext, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<FinanceItem>> context)
    {
        using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var correlationId = context.Message.CorrelationId;
        foreach (var item in context.Message.DataObjects)
        {
            var typedItem = JsonSerializer.Deserialize<FinanceItem>(item.Data);
            switch (item.CRUD)
            {
                case CRUD.Create:
                    //добавляем новое поступление денег на счет
                    await _dbContext.FinanceItems.AddAsync(typedItem);
                    break;
                case CRUD.Update:
                    //обновляем запись текущего баланса
                    var finItem = await _dbContext.FinanceItems.Where(p => p.UserLogin == typedItem.UserLogin &&
                        p.Status == FinanceRecordStatus.Баланс).FirstOrDefaultAsync();
                    if (finItem == null)
                    {
                        await _dbContext.FinanceItems.AddAsync(typedItem);
                    }
                    else
                    {
                        finItem.Value = typedItem.Value;
                    }
                    break;
                case CRUD.Delete:
                    //удаляем запись
                    var delItem = await _dbContext.FinanceItems.FindAsync(typedItem.FinanceId);
                    if (delItem == null)
                    {
                        throw new Exception($"Запись для удаления не найдена");
                    }
                    _dbContext.FinanceItems.Remove(delItem);
                    break;
            }
        }
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
