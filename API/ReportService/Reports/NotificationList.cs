using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.Notification;
using Common.Contracts.Report;
using ReportService.DTO;
using ReportService.DTO.ResultDTO;
using ReportService.Services;

namespace ReportService.Reports;

public class NotificationList
{
    private readonly GrpcReportsClient _client;
    public NotificationList(GrpcReportsClient client)
    {
        _client = client;
    }

    public Task<NotificationItemsDTO[]> GetNotificationItems(ParamItemDTO[] param)
    {
        var userLoginPar = param.FirstOrDefault(p => p.Id == "UserLogin").Value;
        var auctionPar = param.FirstOrDefault(p => p.Id == "Auction").Value;
        var query = new SqlQuery();
        List<AuctionItem> auctionList;
        List<NotifyItem> notifyList;
        IEnumerable<Guid> ids;
        //первоначально нужно получить массив id-ников, отфильтрованных по указанным данным
        //если указан auctionPar - первоначально производим поиск по аукционам
        //иначе - первоначально производим поиск по уведомлениям
        if (!string.IsNullOrEmpty(auctionPar))
        {
            auctionList = GetAuctionItems(auctionPar, query).GetAwaiter().GetResult().Result;
            ids = auctionList.Select(p => p.ItemId);
            if (ids.Count() == 0)
            {
                return Task.FromResult<NotificationItemsDTO[]>(null);
            }
            //фильтруем по полученным id-никам
            notifyList = GetNotifyItemsById(userLoginPar, ids, query).GetAwaiter().GetResult().Result;
        }
        else
        {
            notifyList = GetNotifyItems(userLoginPar, query).GetAwaiter().GetResult().Result;
            ids = notifyList.Select(p => p.ItemId);
            if (ids.Count() == 0)
            {
                return Task.FromResult<NotificationItemsDTO[]>(null);
            }
            //фильтруем по полученным id-никам
            auctionList = GetAuctionItemsById(auctionPar, ids, query).GetAwaiter().GetResult().Result;
        }

        //объединяем оба набора данных
        var result = from auction in auctionList
                     join notify in notifyList
                     on auction.ItemId equals notify.ItemId
                     select new NotificationItemsDTO
                     {
                         ItemId = auction.ItemId,
                         UserLogin = notify.UserLogin,
                         Title = auction.Title
                     };
        return Task.FromResult(result.ToArray());
    }

    private async Task<ApiResponse<List<NotifyItem>>> GetNotifyItems(string userLoginPar, SqlQuery query)
    {
        query.Text = "select * from \"NotifyItems\" where true";
        query.Parameters.Clear();
        //получаем список уведомлений для заданного пользователя (или для всех, если никто не указан)
        //также дополнительный фильтр по наименованию аукциона
        var notifyTask = Task.Run(() =>
        {
            if (!string.IsNullOrEmpty(userLoginPar))
            {
                query.Text += " and \"UserLogin\" ilike {0}";
                query.Parameters.Add("%" + userLoginPar + "%");
            }
            query.Text += " limit 100";
            return _client.GetNotificationReportItems(System.Text.Json.JsonSerializer.Serialize(query));
        });
        return await notifyTask;
    }

    private async Task<ApiResponse<List<AuctionItem>>> GetAuctionItems(string auctionPar, SqlQuery query)
    {
        query.Text = "select * from \"SearchItems\" where true";
        query.Parameters.Clear();
        var auctionTask = Task.Run(() =>
        {
            if (!string.IsNullOrEmpty(auctionPar))
            {
                query.Text += " and \"Title\" ilike {0}";
                query.Parameters.Add("%" + auctionPar + "%");
            }
            query.Text += " order by \"ItemId\" limit 100";
            return _client.GetAuctionReportItems(System.Text.Json.JsonSerializer.Serialize(query));
        });
        return await auctionTask;
    }

    private async Task<ApiResponse<List<AuctionItem>>> GetAuctionItemsById(string auctionPar, IEnumerable<Guid> notifyIds, SqlQuery query)
    {
        query.Text = "select * from \"SearchItems\" where \"ItemId\" in (";
        query.Text += string.Join(',', notifyIds.Select(p => "'" + p + "'"));
        query.Text += ")";
        query.Parameters.Clear();
        var auctionTask = Task.Run(() =>
        {
            if (!string.IsNullOrEmpty(auctionPar))
            {
                query.Text += " and \"Title\" ilike {0}";
                query.Parameters.Add("%" + auctionPar + "%");
            }
            query.Text += " order by \"ItemId\" limit 100";
            return _client.GetAuctionReportItems(System.Text.Json.JsonSerializer.Serialize(query));
        });
        return await auctionTask;
    }

    private async Task<ApiResponse<List<NotifyItem>>> GetNotifyItemsById(string userLoginPar, IEnumerable<Guid> auctionIds, SqlQuery query)
    {
        query.Text = "select * from \"NotifyItems\" where \"ItemId\" in (";
        query.Text += string.Join(',', auctionIds.Select(p => "'" + p + "'"));
        query.Text += ")";
        query.Parameters.Clear();
        var auctionTask = Task.Run(() =>
        {
            if (!string.IsNullOrEmpty(userLoginPar))
            {
                query.Text += " and \"UserLogin\" ilike {0}";
                query.Parameters.Add("%" + userLoginPar + "%");
            }
            query.Text += " limit 100";
            return _client.GetNotificationReportItems(System.Text.Json.JsonSerializer.Serialize(query));
        });
        return await auctionTask;
    }
}
