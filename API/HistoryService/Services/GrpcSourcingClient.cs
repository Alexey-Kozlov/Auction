using System.Text.Json;
using Common.Contracts.EventSourcing;
using Grpc.Core;
using Grpc.Net.Client;
using SourcingService;

namespace HistoryService.Services;

public class GrpcSourcingClient
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Metadata headers = new();
    public GrpcSourcingClient(IConfiguration config, IHttpContextAccessor httpContextAccessor)
    {
        _config = config;
        _httpContextAccessor = httpContextAccessor;
        //добавляем авторизацию в GRPC-запрос из полученного http-контекста, иначе будет 401 ответ
        headers.Add("Authorization", $"{_httpContextAccessor.HttpContext.Request.Headers["Authorization"]}");
    }

    public async Task<List<EventsLog>> GetHistoryItems(Guid auctionId)
    {
        var channel = GrpcChannel.ForAddress(_config["GrpcSourcing"], new GrpcChannelOptions
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        var client = new GrpcSourcing.GrpcSourcingClient(channel);
        var request = new GetHistoryRequest { HistoryRequest = auctionId.ToString() };
        try
        {
            var reply = await client.GetHistoryAsync(request, headers);
            return JsonSerializer.Deserialize<List<EventsLog>>(reply.HistoryResponse);
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

