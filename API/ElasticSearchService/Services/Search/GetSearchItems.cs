using Common.Contracts.Auction;
using Common.Contracts.Communication;
using Common.Contracts.ELKSearch;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace ElasticSearchService.Services.Search;

public class GetSearchItems
{
    private readonly ElkClient _client;
    private readonly IConfiguration _configuration;
    public GetSearchItems(ElkClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public async Task<Dictionary<Guid, string>> GetAuctionIds(ElkSearchRequest context)
    {
        //запрос на получение списка всех найденных Id аукционов в индексе аукционов
        //получение всех идентификаторов необходимо чтобы потом объединить с результатами
        //поиска в индексе чатов и исключить дубли

        var docs = await _client.Client.SearchAsync<AuctionCreatingElk>(s => s
        //в запросе указываем получить только ID аукционов - это поле itemId в индексе
            .Source(new SourceConfig(new SourceFilter
            {
                Includes = Fields.FromStrings(["itemId"])
            }))
            .Size(int.Parse(_configuration["SearchSizeLimit"]))
            //запрос - поисковый запрос разбивается на термы, все термы должны быть
            //указанном поле. Поиск нечеткий (Fuzzy), с учетом русского языка.
            //поиск по ИЛИ в 3-х полях - Title, Properties, Description            
            .Query(q => q
                .Bool(b => b
                    .Should(s => s
                       .Match(m => m
                           .Field(f => f.Title)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(context.SearchTerm)
                            .Operator(Operator.And)
                        ),
                        s => s
                       .Match(m => m
                           .Field(f => f.Description)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(context.SearchTerm)
                            .Operator(Operator.And)
                        ),
                        s => s
                       .Match(m => m
                           .Field(f => f.Properties)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(context.SearchTerm)
                            .Operator(Operator.And)
                        )
                    )
                )
            )
        );

        var rezult = new Dictionary<Guid, string>();
        foreach (var auctionId in docs.Documents.Select(p => p.ItemId))
        {
            rezult.Add(auctionId, auctionId.ToString());
        }
        return rezult;
    }

    public async Task<List<string>> GetChatIds(ElkSearchRequest context, Dictionary<Guid, string> auctionIds)
    {
        //запрос на получение количества возвращаемых записей (в чатах)
        var docs = await _client.CommunicationClient.SearchAsync<CommunicationSearch>(s => s
            .Size(int.Parse(_configuration["SearchSizeLimit"]))
            .Source(new SourceConfig(new SourceFilter
            {
                Includes = Fields.FromStrings(["auctionId"])
            }))
            .Query(q => q
                .Bool(b => b
                    .Should(s => s
                       .Match(m => m
                           .Field(f => f.Message)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(context.SearchTerm)
                            .Operator(Operator.And)
                        )
                    )
                )
            )
        );

        foreach (var chatId in docs.Documents.Select(p => p.AuctionId))
        {
            if (!auctionIds.ContainsKey(chatId))
            {
                auctionIds.Add(chatId, chatId.ToString());
            }
        }
        var rezult = new List<string>();
        rezult.AddRange(auctionIds.Values);
        return rezult;
    }
}