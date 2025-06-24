using Common.Contracts;
using Common.Contracts.Finance;
using Common.Utils.Extentions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class FinanceHistoryConsumer : IConsumer<FinanceSortRequest>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly SearchDbContext _dbContext;
    public FinanceHistoryConsumer(IPublishEndpoint publishEndpoint, SearchDbContext dbContext)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<FinanceSortRequest> context)
    {
        var ids = context.Message.FinanceItems.Where(p => p.AuctionId.HasValue)
            .Select(p => p.AuctionId).ToArray();
        var auctionList = await _dbContext.AuctionItems.Where(p => ids.Contains(p.AuctionId))
        .Select(p => new { p.AuctionId, p.Title, p.Seller }).ToListAsync();
        //объединяем результаты - исходные записи финансов и дополняем записями аукционов (поля Seller и Title)
        var rezult = context.Message.FinanceItems.LeftOuterJoin(
            auctionList,
            leftKey => leftKey.AuctionId,
            rightKey => rightKey.AuctionId,
            (fin, auction) => new FinanceHistoryItem
            {
                ActionDate = fin.ActionDate,
                AuctionId = fin.AuctionId,
                AuctionSeller = auction?.Seller,
                AuctionTitle = auction?.Title,
                ItemId = fin.ItemId,
                Status = fin.Status,
                UserLogin = fin.UserLogin,
                Value = fin.Value
            }
        );
        //сортируем по заданному полю
        rezult = context.Message.OrderBy switch
        {
            "titleAsc" => rezult.OrderBy(p => p.AuctionTitle).ThenBy(p => p.ItemId),
            "titleDesc" => rezult.OrderByDescending(p => p.AuctionTitle).ThenBy(p => p.ItemId),
            "actionDateAsc" => rezult.OrderBy(p => p.ActionDate).ThenBy(p => p.ItemId),
            "actionDateDesc" => rezult.OrderByDescending(p => p.ActionDate).ThenBy(p => p.ItemId),
            "sellerAsc" => rezult.OrderBy(p => p.AuctionSeller).ThenBy(p => p.ItemId),
            "sellerDesc" => rezult.OrderByDescending(p => p.AuctionSeller).ThenBy(p => p.ItemId),
            "statusAsc" => rezult.OrderBy(p => p.Status).ThenBy(p => p.ItemId),
            "statusDesc" => rezult.OrderByDescending(p => p.Status).ThenBy(p => p.ItemId),
            "valueAsc" => rezult.OrderBy(p => p.Value).ThenBy(p => p.ItemId),
            "valueDesc" => rezult.OrderByDescending(p => p.Value).ThenBy(p => p.ItemId),
            _ => rezult.OrderBy(p => p.ItemId)
        };
        var pageCount = (rezult.Count() + context.Message.PageSize - 1) / context.Message.PageSize;
        var totalCount = rezult.Count();
        //добавляем пагинацию
        rezult = rezult.Skip((context.Message.PageNumber - 1) * context.Message.PageSize)
            .Take(context.Message.PageSize);
        var message = new ApiResponse<PagedResult<List<FinanceHistoryItem>>>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = new PagedResult<List<FinanceHistoryItem>>()
            {
                Results = rezult.ToList(),
                PageCount = pageCount,
                TotalCount = totalCount,
                SessionId = context.Message.SessionId
            }
        };
        //возвращаем результат - на сервис уведомлений для последующей пересылке на фронт
        await _publishEndpoint.Publish(message);
    }
}