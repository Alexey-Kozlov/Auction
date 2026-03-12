using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Communication;
using Common.Contracts.Report;
using ReportService.DTO;
using ReportService.Services;

namespace ReportService.Reports;

public class Comments
{
    private readonly GrpcReportsClient _client;
    public Comments(GrpcReportsClient client)
    {
        _client = client;
    }

    public Task<AuctionTreeItemCommunication[]> GetCommentsItems(ParamItemDTO[] param)
    {
        var sellerPar = param.FirstOrDefault(p => p.Id == "Seller").Value;
        var commentPar = param.FirstOrDefault(p => p.Id == "Comment").Value;
        var searchText = param.FirstOrDefault(p => p.Id == "SearchText").Value;
        var query = new SqlQuery();
        List<AuctionItem> auctionList;
        List<CommunicationItem> communicationList;
        //получаем список аукционов для заданного автора аукциона (или для всех, если никто не указан)
        query.Text = "select * from \"SearchItems\" where true";

        if (!string.IsNullOrEmpty(searchText))
        {
            query.Text += " and (\"Title\" ilike {0} or \"Properties\" ilike {0} or \"Description\" ilike {0})";
            query.Parameters.Add("%" + searchText + "%");
        }

        if (!string.IsNullOrEmpty(sellerPar))
        {
            var parNumber = string.IsNullOrEmpty(searchText) ? "0" : "1";
            query.Text += " and \"Seller\" ilike {" + parNumber + "}";
            query.Parameters.Add("%" + sellerPar + "%");
        }

        query.Text += " order by \"ItemId\" limit 1000";
        auctionList = _client.GetAuctionReportItems(JsonSerializer.Serialize(query))
            .GetAwaiter().GetResult().Result;

        if (auctionList.Count() == 0) return Task.FromResult<AuctionTreeItemCommunication[]>(null);

        // делаем дополнительный запрос к микросервису чата для получения списка комментариев для аукционов
        query.Text = "select * from \"CommunicationItems\" where true";
        query.Parameters.Clear();
        // если были параметры фильтрации - отбираем комменты по возвращенным id-никам, типа GUID
        // если не было фильтрации - отменяем фильтрацию по полученным id-никам - нет смысла
        if (!string.IsNullOrEmpty(sellerPar) || !string.IsNullOrEmpty(searchText))
        {
            query.Text += " and \"AuctionId\" in (";
            query.Text += string.Join(',', auctionList.Select(p => "'" + p.ItemId + "'")) + ")";
        }
        // если был параметр фильтрации по тексту сообщения
        if (!string.IsNullOrEmpty(commentPar))
        {
            query.Text += " and \"Message\" ilike {0}";
            query.Parameters.Add("%" + commentPar + "%");
        }

        query.Text += " limit 3000";
        communicationList = _client.GetCommunicationReportItems(JsonSerializer.Serialize(query))
            .GetAwaiter().GetResult().Result;
        var rezult = new List<AuctionTreeItemCommunication>();

        // отбираем уникальные записи аукционов, у которых есть комментарии
        var _tmp = auctionList.Join(communicationList,
            auction => auction.ItemId,
            comment => comment.AuctionId,
            (auction, comment) => new AuctionTreeItemCommunication
            {
                key = auction.ItemId,
                data = new AuctionTreeItemData
                {
                    auctionEnd = auction.AuctionEnd,
                    createAt = auction.CreateAt,
                    seller = auction.Seller,
                    title = auction.Title,
                    itemid = auction.ItemId
                },
                children = communicationList.Where(b => b.AuctionId == auction.ItemId).Select(p =>
                    new CommunicationTreeItem
                    {
                        key = p.ItemId.Value,
                        data = new CommunicationTreeItemData
                        {
                            author = p.UserLogin,
                            createAt = p.UpdateAt,
                            comment = p.Message
                        }
                    }).ToList()
            }
        ).DistinctBy(p => p.key).OrderBy(p => p.data.title);
        rezult.AddRange(_tmp);

        return Task.FromResult(rezult.ToArray());
    }
}

