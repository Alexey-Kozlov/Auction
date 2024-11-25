using System.Reflection;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class FinanceCreateProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public FinanceCreateProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task ProcessESLog(ConsumeContext<ESContract> context)
    {
        //В процедуре Postgres делаем:
        //- запись в ES лог о создании поступления денег
        //Формирование списка корректирующих записей:
        //- запись о поступления денег - для создания записи в сервисе FinanceService
        //- запись об обновленном балансе - для обновления баланса в сервисе FinanceService
        var result = await _dbContext.finance_create(
            context.Message.CorrelationId,
            context.Message.EventData,
            context.Message.UserLogin).ToListAsync();
        //возвращаем список записей для изменения соответствующих БД в нужных сервисах
        var listItems = new DataForProcessingServicesList
        {
            DataObjects = new List<DataForProcessingService>()
        };
        foreach (var item in result)
        {
            listItems.DataObjects.Add
            (
                new DataForProcessingService
                {
                    DataType = item.entitytype,
                    Data = item.eventdata,
                    CRUD = (CRUD)item.crud
                }
            );
        }
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
        sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
        await _publishEndpoint.Publish(sendObject);
    }
}