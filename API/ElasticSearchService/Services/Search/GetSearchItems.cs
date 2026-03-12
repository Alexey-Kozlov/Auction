using Common.Contracts.Auction;
using Common.Contracts.Communication;
using Common.Contracts.ELKSearch;
using Common.Contracts.Tag;
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

    //получение AuctionId в списке аукционов
    public async Task<List<string>> GetAuctionIds(ElkSearchRequest context)
    {
        //запрос на получение списка всех найденных Id аукционов в индексе аукционов
        //получение всех идентификаторов необходимо чтобы потом объединить с результатами
        //поиска в индексе чатов и тегов и исключить дубли

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

        var rezult = new List<string>();
        rezult.AddRange(docs.Documents.Select(p => p.ItemId.ToString()));
        return rezult;
    }

    //добавляем уникальные AuctionId из чатов
    public async Task<List<string>> GetChatIds(ElkSearchRequest context, List<string> auctionIds)
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

        foreach (var chatId in docs.Documents.Select(p => p.AuctionId.ToString()))
        {
            if (!auctionIds.Any(p => p == chatId))
            {
                auctionIds.Add(chatId);
            }
        }
        return auctionIds;
    }

    //добавляем уникальные AuctionId из тегов
    public async Task<List<string>> GetTagIds(ElkSearchRequest context, List<string> auctionIds)
    {
        //запрос на получение количества возвращаемых записей (в тегах)
        var docs = await _client.TagClient.SearchAsync<TagSearch>(s => s
            .Size(int.Parse(_configuration["SearchSizeLimit"]))
            .Source(new SourceConfig(new SourceFilter
            {
                Includes = Fields.FromStrings(["auctionId"])
            }))
            .Query(q => q
                .Bool(b => b
                    .Should(s => s
                       .Match(m => m
                           .Field(f => f.Tag)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(context.SearchTerm)
                            .Operator(Operator.And)
                        )
                    )
                )
            )
        );

        foreach (var tagId in docs.Documents.Select(p => p.AuctionId.ToString()))
        {
            if (!auctionIds.Any(p => p == tagId))
            {
                auctionIds.Add(tagId);
            }
        }
        return auctionIds;
    }
}