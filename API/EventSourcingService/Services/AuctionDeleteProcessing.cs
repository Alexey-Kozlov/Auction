using Common.Contracts.EventSourcing;
using EventSourcingService.Data;
using MassTransit;

namespace EventSourcingService.Services;

public class AuctionDeleteProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly InsertItemToEventSourcing _insertItemToEventSourcing;

    public AuctionDeleteProcessing(IPublishEndpoint publishEndpoint, InsertItemToEventSourcing insertItemToEventSourcing,
        EventSourcingDbContext dbContext)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _insertItemToEventSourcing = insertItemToEventSourcing;
    }

    public async Task Processing(ConsumeContext<ESContract> context)
    {
        //В процедуре Postgres делаем:
        //- запись в ES лог об удалении аукциона
        //- запись в ES лог об удалении последнего платежа (если были ставки)
        //- записи в ES лог об удалении всех ставок (если были)
        //- записи в ES лог об удалении всех уведомлений (если были)
        //Формирование списка корректирующих записей:
        //- запись удаленного аукциона - для удаления из сервиса SearchService
        //- если были - запись удаленного платежа - для удаления из сервиса FinanceService у соответствующего пользователя
        //- если были - запись обновления денежного баланса - для обновления баланса в сервисе FinanceService у соответствующего пользователя
        //- если были - записи удаленных ставок - для удаления из сервиса BiddingService
        //- если были - записи удаленных уведомлений - для удаления из сервиса NotificationService
        var result = await _dbContext.auction_delete()
    }

}