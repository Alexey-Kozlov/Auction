using Common.Utils;
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

    public async Task<ApiResponse<List<Item>>> GetItemsForSearchDb()
    {
        return await _client.GetFromJsonAsync<ApiResponse<List<Item>>>(_config["AuctionServiceUrl"] + "/api/auctions");
    }
}
