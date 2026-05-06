using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Report;
using Grpc.Core;
using Grpc.Net.Client;
using TagService;

namespace SearchService.Services;

public class GrpcTagClient
{
    private readonly IConfiguration _config;
    public GrpcTagClient(IConfiguration config)
    {
        _config = config;
    }

    public async Task<ApiResponse<Guid[]>> GetTagAuctionItems(SqlQuery query)
    {
        var channel = GrpcChannel.ForAddress(_config["GrpcTagSearch"], new GrpcChannelOptions
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        var client = new GrpcSearch.GrpcSearchClient(channel);
        var request = new GetTagAuctionsRequest { TagAuctionsRequest = JsonSerializer.Serialize(query) };
        try
        {
            var reply = await client.GetTagAuctionsAsync(request);
            var result = JsonSerializer.Deserialize<ApiResponse<Guid[]>>(reply.Rezult);
            return result;
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка GRPC - {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Невозможно вызвать GrpcTagSearch сервер - {ex.Message}");
            return null;
        }
    }
}

