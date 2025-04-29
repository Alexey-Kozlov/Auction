using System.Reflection;
using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.ELKSearch;
using Common.Contracts.Processing;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class SearchCreatingElkConsumer : IConsumer<DataForProcessingServicesList<ElkSearchCreating>>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;
    private readonly IConfiguration _configuration;

    public SearchCreatingElkConsumer(IPublishEndpoint publishEndpoint, ElkClient client, IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<ElkSearchCreating>> context)
    {
        try
        {
            var typed = JsonSerializer.Deserialize<ElkSearchCreating>(context.Message.DataObjects[0].Data);
            //запрос на получение количества возвращаемых записей
            var countResponse = await _client.Client.CountAsync<AuctionCreatingElk>(s => s
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
            if (!countResponse.IsValidResponse)
            {
                var error = new LoggingServiceError
                {
                    CorrelationId = Guid.NewGuid(),
                    ErrorExceptionMessage = string.Join(",", countResponse.ElasticsearchServerError.Error.RootCause),
                    ErrorMessage = countResponse.DebugInformation,
                    IsError = true,
                    ErrorServiceName = "ElasticSearch_SearchCreatingElk"
                };
                await _publishEndpoint.Publish(error);
                throw new Exception(countResponse.DebugInformation);
            }
            //итоговый запрос на получение записей
            var elkResponse = await _client.Client.SearchAsync<AuctionCreatingElk>(s => s
                .From((typed.PageNumber - 1) * typed.PageSize)
                .Size(typed.PageSize)
                .TrackTotalHits(new Elastic.Clients.Elasticsearch.Core.Search.TrackHits(true))
                //.Sort()
                //.Sort(p => p.Field(f => f.Title, f => f.Order(SortOrder.Asc)))
                // .Sort(p => p.Field("title", fs => fs.Order(SortOrder.Asc)
                // .UnmappedType(Elastic.Clients.Elasticsearch.Mapping.FieldType.Keyword)))
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
                //сортируем сначала по наименованию аукциона, потом по id (если одинаковые наименования)
                //Suffix - смотрим определение индекса - mappings в формате json, значение Suffix - keyword -
                //наименование свойства после "fields"
                ).Sort(p => p.Field(f => f.Title.Suffix("keyword"), fs => fs.Order(SortOrder.Asc)),
                p => p.Field(f => f.AuctionId.Suffix("keyword"), fs => fs.Order(SortOrder.Asc)))
            );
            if (!elkResponse.IsValidResponse)
            {
                var error = new LoggingServiceError
                {
                    CorrelationId = Guid.NewGuid(),
                    ErrorExceptionMessage = string.Join(",", elkResponse.ElasticsearchServerError.Error.RootCause),
                    ErrorMessage = elkResponse.DebugInformation,
                    IsError = true,
                    ErrorServiceName = "ElasticSearch_SearchCreatingElk"
                };
                await _publishEndpoint.Publish(error);
                throw new Exception(countResponse.DebugInformation);
            }
            var itemsCount = (int)countResponse.Count;
            var pageCount = 0;
            if (itemsCount > 0)
            {
                pageCount = (itemsCount + typed.PageSize - 1) / typed.PageSize;
            }

            var resultResponse = new ApiResponse<PagedResult<List<AuctionCreatingElk>>>
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Result = new PagedResult<List<AuctionCreatingElk>>()
                {
                    Results = elkResponse.IsValidResponse ? elkResponse.Documents.ToList() : new List<AuctionCreatingElk>(),
                    PageCount = pageCount,
                    TotalCount = itemsCount
                }
            };

            var resultData = new ElkSearchResult
            {
                CorrelationId = context.Message.CorrelationId,
                Result = resultResponse,
                IsError = false
            };
            await _publishEndpoint.Publish(resultData);
        }
        catch (Exception e)
        {
            //ошибки, в т.ч. штатные
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, e.Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "ElkService_SearchCreatingELK");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);
            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);
            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }
}
