using System.Reflection;
using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.ELKSearch;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using Elastic.Clients.Elasticsearch;
using ElasticSearchService.Consumers.Search;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class SearchCreatingElkConsumer : IConsumer<DataForProcessingServicesList<ElkSearchCreating>>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;
    private readonly GetSearchItems _searchItems;
    private readonly IConfiguration _configuration;

    public SearchCreatingElkConsumer(IPublishEndpoint publishEndpoint, ElkClient client,
        IConfiguration configuration, GetSearchItems searchItems)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
        _configuration = configuration;
        _searchItems = searchItems;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<ElkSearchCreating>> context)
    {
        try
        {
            var typed = JsonSerializer.Deserialize<ElkSearchCreating>(context.Message.DataObjects[0].Data);

            //получаем идентификаторы аукционов, где встречается ичскомая фраза - в аукционах и чатах
            var auctionIds = await _searchItems.GetAuctionIds(typed);
            var searchIds = await _searchItems.GetChatIds(typed, auctionIds);

            //итоговый запрос на получение записей - ищем по полученному списку AuctionId
            var elkResponse = await _client.Client.SearchAsync<AuctionCreatingElk>(s => s
                .From((typed.PageNumber - 1) * typed.PageSize)
                .Size(typed.PageSize)
                .TrackTotalHits(new Elastic.Clients.Elasticsearch.Core.Search.TrackHits(true))
                .Query(q => q.TermsSet(p => p.Field(r => r.ItemId.Suffix("keyword")).Terms(searchIds)
                    .MinimumShouldMatch(1))
                //сортируем сначала по наименованию аукциона, потом по id (если одинаковые наименования)
                //Suffix - смотрим определение индекса - mappings в формате json, значение Suffix - keyword -
                //наименование свойства после "fields"
                ).Sort(p => p.Field(f => f.Title.Suffix("keyword"), fs => fs.Order(SortOrder.Asc)),
                p => p.Field(f => f.ItemId.Suffix("keyword"), fs => fs.Order(SortOrder.Asc)))
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
                //throw new Exception(countResponse.DebugInformation);
            }
            var itemsCount = 1;// (int)countResponse.Count;
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
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetInnerException(e).Message);
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
