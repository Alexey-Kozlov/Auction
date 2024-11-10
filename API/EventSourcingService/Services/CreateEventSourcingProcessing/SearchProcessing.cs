using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class SearchProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly InsertItemToEventSourcing _insertItemToEventSourcing;

    public SearchProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
    InsertItemToEventSourcing insertItemToEventSourcing)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _insertItemToEventSourcing = insertItemToEventSourcing;
    }
    public async Task Processing(ConsumeContext<ESContract> context)
    {
        switch (context.Message.OperationType)
        {
            case OperationType.Update:
            case OperationType.Insert:
            case OperationType.Delete:
                await AuctionAction(context.Message);
                break;
            case OperationType.Bid:
                await BidAction(context.Message);
                break;
        }
    }

    private async Task BidAction(ESContract context)
    {
        //делаем запись в ES об обновлении Entity AuctionItem
        var auctionItem = JsonSerializer.Deserialize<AuctionItem>(context.EventData);

        //проверки на наличие обновляемой или удаляемой записей
        var lastSnapShotId = await _dbContext.EventsLogs.Where(p => p.SnapShotId != null)
            .OrderBy(p => p.CreateAt).FirstOrDefaultAsync();
        //получаем из EventSourcing записи по BidItem по данному пользователю, SnapShot
        var auctionBidItem = await _dbContext.EventsLogs.Where(p =>
            (p.SnapShotId == lastSnapShotId.SnapShotId || p.SnapShotId == null) &&
            p.EntityType == nameof(AuctionItem) &&
            p.AuctionId == context.AuctionId &&
            p.CreateAt >= lastSnapShotId.CreateAt
        ).OrderByDescending(p => p.Version).FirstOrDefaultAsync();
        if (auctionBidItem == null || auctionBidItem.OperationType == OperationType.Delete)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка обновления записи - запись аукциона " + context.AuctionId + " не найдена.");
            throw new Exception($"{DateTime.Now} Ошибка обновления записи - запись аукциона " + context.AuctionId + " не найдена.");
        }
        //записали в ES лог действие
        await _insertItemToEventSourcing.Processing(context);
        //делаем объект на изменение данных в сервисе SearchService
        var updateItem = new ActionMessageList<AuctionItem>
        {
            ActionItemsList = new List<ActionMessage<AuctionItem>>
            {
                new ActionMessage<AuctionItem>
                {
                    ActionItem = auctionItem,
                    OperationType = context.OperationType,
                    CorrelationId = context.CorrelationId
                }
            },
            CallBackType = context.CallBackType
        };
        //посылаем в сервис SearchService для обновления в БД сервиса
        await _publishEndpoint.Publish(updateItem);
    }
    private async Task AuctionAction(ESContract context)
    {
        //делаем запись в ES об обновлении Entity AuctionItem
        var auctionItem = JsonSerializer.Deserialize<AuctionItem>(context.EventData);

        if (context.OperationType == OperationType.Delete || context.OperationType == OperationType.Update)
        {
            //проверки на наличие обновляемой или удаляемой записей
            var lastSnapShotId = await _dbContext.EventsLogs.Where(p => p.SnapShotId != null)
                .OrderBy(p => p.CreateAt).FirstOrDefaultAsync();
            //получаем из EventSourcing записи по BidItem по данному пользователю, SnapShot
            var auctionBidItem = await _dbContext.EventsLogs.Where(p =>
                (p.SnapShotId == lastSnapShotId.SnapShotId || p.SnapShotId == null) &&
                p.EntityType == nameof(AuctionItem) &&
                p.AuctionId == context.AuctionId &&
                p.CreateAt >= lastSnapShotId.CreateAt
            ).OrderByDescending(p => p.Version).FirstOrDefaultAsync();
            if (auctionBidItem == null || auctionBidItem.OperationType == OperationType.Delete)
            {
                Console.WriteLine($"{DateTime.Now} Ошибка обновления записи - запись аукциона " + context.AuctionId + " не найдена.");
                throw new Exception($"{DateTime.Now} Ошибка обновления записи - запись аукциона " + context.AuctionId + " не найдена.");
            }
        }

        //записали в ES лог действие
        await _insertItemToEventSourcing.Processing(context);
        //делаем объект на изменение данных в сервисе SearchService
        var updateItem = new ActionMessageList<AuctionItem>
        {
            ActionItemsList = new List<ActionMessage<AuctionItem>>
            {
                new ActionMessage<AuctionItem>
                {
                    ActionItem = auctionItem,
                    OperationType = context.OperationType,
                    CorrelationId = context.CorrelationId
                }
            },
            CallBackType = context.CallBackType
        };
        //посылаем в сервис SearchService для обновления в БД сервиса
        await _publishEndpoint.Publish(updateItem);
    }
}