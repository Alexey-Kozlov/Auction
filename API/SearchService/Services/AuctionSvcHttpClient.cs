using Common.Contracts;
using Common.Utils;

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

    public async Task<ApiResponse<List<AuctionItem>>> GetItemsForSearchDb()
    {
        return await _client.GetFromJsonAsync<ApiResponse<List<AuctionItem>>>(_config["AuctionServiceUrl"] + "/api/auctions");
    }
}
