using SearchService.Entities;

namespace SearchService.Services;

public class AuctionSvcHttpClient
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public AuctionSvcHttpClient(HttpClient client, IConfiguration config)
    {
        _client = client;
        _config = config;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        return await _client.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"] + "/api/auctions");
    }
}
