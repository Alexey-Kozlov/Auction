using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.ELKSearch;
using ElkSearchService;
using Grpc.Core;
using Grpc.Net.Client;

namespace SearchService.Services;

public class GrpcElkClient
{
    private readonly IConfiguration _config;

    public GrpcElkClient(IConfiguration config)
    {
        _config = config;
    }

    public async Task<ApiResponse<PagedResult<List<AuctionItem>>>> GetElkSearchItems(ElkSearchRequest elkRequest)
    {
        var channel = GrpcChannel.ForAddress(_config["GrpcElkSearch"], new GrpcChannelOptions
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        var client = new GrpcElk.GrpcElkClient(channel);
        var request = new GetElkSearchRequest { ElkSearchRequest = JsonSerializer.Serialize(elkRequest) };

        try
        {
            var reply = await client.GetElkSearchAsync(request);
            var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<List<AuctionItem>>>>(reply.ElkSearchResult);
            return result;
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка GRPC - {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Невозможно вызвать GrpcElkSearch сервер - {ex.Message}");
            return null;
        }
    }

}
