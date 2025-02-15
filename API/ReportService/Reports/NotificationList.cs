using System.Linq.Expressions;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using ReportService.Services;
using ReportService.DTO;
using Serialize.Linq.Serializers;
using System.Runtime.Serialization;
using Common.Utils.Extentions;
using Common.Contracts.Notification;

namespace ReportService.Reports;

[KnownType(typeof(List<Guid>))]
public class NotificationList
{
    private readonly IServiceProvider _services;
    public NotificationList(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<string> GetNotificationItems(ParamItem[] param)
    {
        var serializer = new ExpressionSerializer(new JsonSerializer())
        {
            AutoAddKnownTypesAsListTypes = true
        };
        using var scope = _services.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<HttpClientService>();

        //получаем список уведомлений для заданного пользователя (или для всех, если никто не указан)
        var notifyTask = Task.Run(() =>
        {
            var par = param.FirstOrDefault(p => p.Id == "UserLogin").Value;
            Expression<Func<NotifyItem, bool>> notifyExp = item => item.UserLogin.Contains(par);
            if (string.IsNullOrEmpty(par))
            {
                notifyExp = item => true;
            }

            var notifyExp_text = serializer.SerializeText(notifyExp);
            return httpClient.GetNotificationItems(notifyExp_text);
        });
        var notifyList = await notifyTask;

        //получаем список аукционам по найденным уведомлениям

        //запрос фильтрации по списку id-ников, ids - список id-ников типа GUID
        var ids = notifyList.Result.Select(p => p.AuctionId).ToList();
        var auctionTask = Task.Run(() =>
        {
            var filterField = "AuctionId";
            var eParam = Expression.Parameter(typeof(AuctionItem), "e");
            var method = ids.GetType().GetMethod("Contains");
            var call = Expression.Call(Expression.Constant(ids), method, Expression.Property(eParam, filterField));
            var auctionExp = Expression.Lambda<Func<AuctionItem, bool>>(call, eParam);
            var auctionExp_text = serializer.SerializeText(auctionExp);
            return httpClient.GetAuctionItems(auctionExp_text);
        });
        var auctionList = await auctionTask;

        var result = from auction in auctionList.Result
                     join notify in notifyList.Result
                     on auction.AuctionId equals notify.AuctionId
                     orderby notify.UserLogin, auction.Title
                     select new
                     {
                         auction.AuctionId,
                         notify.UserLogin,
                         auction.Title
                     };

        return System.Text.Json.JsonSerializer.Serialize(result, result.GetType());
    }

}
