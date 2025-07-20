using Common.Contracts.Auction;
using Common.Contracts.Communication;
using Common.Contracts.ELKSearch;
using Common.Contracts.Processing;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers.Search;

public class GetSearchItems
{
    private readonly ElkClient _client;
    private readonly IPublishEndpoint _publishEndpoint;
    public GetSearchItems(ElkClient client, IPublishEndpoint publishEndpoint)
    {
        _client = client;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Dictionary<Guid, string>> GetAuctionIds(ElkSearchCreating typed)
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
            //запрос - поисковый запрос разбивается на термы, все термы должны быть
            //указанном поле. Поиск нечеткий (Fuzzy), с учетом русского языка.
            //поиск по ИЛИ в 3-х полях - Title, Properties, Description            
            .Query(q => q
                .Bool(b => b
                    .Should(s => s
                       .Match(m => m
                           .Field(f => f.Title)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(typed.SearchTerm)
                            .Operator(Operator.And)
                        ),
                        s => s
                       .Match(m => m
                           .Field(f => f.Description)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(typed.SearchTerm)
                            .Operator(Operator.And)
                        ),
                        s => s
                       .Match(m => m
                           .Field(f => f.Properties)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(typed.SearchTerm)
                            .Operator(Operator.And)
                        )
                    )
                )
            )
        );
        await CheckSearchErrors(docs);
        var rezult = new Dictionary<Guid, string>();
        foreach (var auctionId in docs.Documents.Select(p => p.ItemId))
        {
            rezult.Add(auctionId, auctionId.ToString());
        }
        return rezult;
    }

    public async Task<List<string>> GetChatIds(ElkSearchCreating typed, Dictionary<Guid, string> auctionIds)
    {
        //запрос на получение количества возвращаемых записей (в чатах)
        var docs = await _client.CommunicationClient.SearchAsync<CommunicationSearch>(s => s
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
                            .Query(typed.SearchTerm)
                            .Operator(Operator.And)
                        )
                    )
                )
            )
        );
        await CheckSearchErrors(docs);
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

    private async Task CheckSearchErrors<T>(SearchResponse<T> auctionItems)
    {
        //проверяем корректность поиска
        if (!auctionItems.IsValidResponse)
        {
            var error = new LoggingServiceError
            {
                CorrelationId = Guid.NewGuid(),
                ErrorExceptionMessage = string.Join(",", auctionItems.ElasticsearchServerError.Error.RootCause),
                ErrorMessage = auctionItems.DebugInformation,
                IsError = true,
                ErrorServiceName = "ElasticSearch_SearchCreatingElk"
            };
            await _publishEndpoint.Publish(error);
            throw new Exception(auctionItems.DebugInformation);
        }
    }
}