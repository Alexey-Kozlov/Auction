using Common.Contracts;
using Common.Contracts.Auction;

namespace SearchService.Services;

//нигде не используется, оставлено для примера - сревис синхронного вызова REST-сервиса
public class AuctionSvcHttpClient
{
    private readonly HttpClient _client;

    public AuctionSvcHttpClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<ApiResponse<List<AuctionItem>>> GetItemsForSearchDb()
    {
        return await _client.GetFromJsonAsync<ApiResponse<List<AuctionItem>>>("http://localhost/api/auctions");
    }
}
