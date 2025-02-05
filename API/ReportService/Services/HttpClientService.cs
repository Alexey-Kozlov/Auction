using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Report;

namespace ReportService.Services;

public class HttpClientService
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public HttpClientService(HttpClient client, IConfiguration config)
    {
        _client = client;
        _config = config;
    }

    public async Task<List<AuctionItem>> GetAuctionItems(string expression)
    {
        using var response = await _client.PostAsJsonAsync(_config["AuctionServiceUrl"], new ReportParamsDTO(expression));
        var auctionItems = await response.Content.ReadFromJsonAsync<List<AuctionItem>>();
        return auctionItems;
    }

    public async Task<List<BidItem>> GetBidItems(string expression)
    {
        using var response = await _client.PostAsJsonAsync(_config["BidServiceUrl"], new ReportParamsDTO(expression));
        var bidItems = await response.Content.ReadFromJsonAsync<List<BidItem>>();
        return bidItems;
    }
}
