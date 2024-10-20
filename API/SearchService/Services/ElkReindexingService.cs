using AutoMapper;
using Common.Contracts;
using MassTransit;

namespace SearchService.Services;

public class ElkReindexingService
{
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public ElkReindexingService(IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    public async Task ReindexElkItems(SendToReindexingElk itemsToIndex)
    {
        var Items = itemsToIndex.AuctionItems.ToList();
        var cnt = 0;
        foreach (var item in Items)
        {
            cnt++;
            var elk = _mapper.Map<AuctionCreatingElk>(item);
            await _publishEndpoint.Publish(new ElkIndexRequest(Guid.NewGuid(), Guid.NewGuid(), elk,
               Items.Count == cnt, cnt, itemsToIndex.SessionId));
            Console.WriteLine($"{DateTime.Now} {cnt} {item.Title}");
        }
    }
}