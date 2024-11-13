using Common.Contracts;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace SearchService.Consumers;

public class RestoreDbSnapShotConsumer : IConsumer<RestoreSnapShotDb>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RestoreDbSnapShotConsumer> _logger;
    private readonly EventSourcingDbContext _context;

    public RestoreDbSnapShotConsumer(IPublishEndpoint publishEndpoint,
        ILogger<RestoreDbSnapShotConsumer> logger, EventSourcingDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _context = context;
    }
    public async Task Consume(ConsumeContext<RestoreSnapShotDb> consumeContext)
    {
        //получаем записи для указанного SnapShot
        var allItems = await _context.EventsLogs.Where(p => p.SnapShotId == consumeContext.Message.SnapShotId).ToListAsync();
        //рассылаем сообщения для восстановления соответствующих БД
        var items = new List<RestoreSnapShotItem>();
        foreach (var projectItem in allItems.Select(p => p.EntityType).Distinct())
        {
            //получили наименования проектов, т.е. БД для восстановления
            items.Clear();
            foreach (var projectItems in allItems.Where(p => p.EntityType == projectItem)
                .Select(p => p.EntityType).Distinct())
            {
                //добавляем запись по типу данных проекта, в каждой записи - все записи БД данного типа (в коллекции Items)
                //для всех проектов в коллекции items будет по 1 записи (Finance, Notification, Search)
                //Для проекта Bidding будут 2 записи - для типов Bid и Auction, в нем 2 такие таблицы нужно восстанавливать
                items.Add(new RestoreSnapShotItem
                {
                    ItemsType = projectItems.EntityType,
                    Items = allItems.Where(p => p.EntityType == projectItem
                        && p.EntityType == projectItems.EntityType)
                        .Select(p => p.EventData).ToList()
                });
            }
            //делаем generic-тип вида RestoreSnapShotItems<наименование проекта>
            //нужно для автоматической передачи сообщений на нужный консьюмер в нужном проекте

            //либо, как вариант - проверять тип и ручками делать нужный тип объекта для рассылки
            //оставил так, на вид сложнее, но более универсальный - не потребуется переписывать при 
            //рассылке сообщений для новых консьюмеров
            Type elementType = Type.GetType("Common.Contracts." + projectItem + ",Contracts", true);
            Type[] types = { elementType };
            Type baseType = typeof(RestoreSnapShotItems<>);
            Type sendType = baseType.MakeGenericType(types);
            //через рефлексию делаем нужный generic-тип
            var sendObject = Activator.CreateInstance(sendType);
            //через рефлексию заполняем нужные свойства
            //можно было назначить интерфейс и заполнять не через рефлексию, это как другой вариант.
            sendObject.GetType().GetProperty("UserLogin").SetValue(sendObject, consumeContext.Message.UserLogin);
            sendObject.GetType().GetProperty("SessionId").SetValue(sendObject, consumeContext.Message.SessionId);
            sendObject.GetType().GetProperty("Items").SetValue(sendObject, items);
            await _publishEndpoint.Publish(sendObject);
        }

        _logger.LogInformation($"{DateTime.Now} --> Рассылка сообщений для восстановления БД, SnapShotId - {consumeContext.Message.SnapShotId}");

    }
}
