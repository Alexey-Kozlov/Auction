using System.Text.Json;
using Grpc.Core;
using Grpc.Net.Client;
using NotifyService;

namespace ReportService.Services;

public class GrpcUsersNotifyClient
{
    private readonly IConfiguration _config;

    public GrpcUsersNotifyClient(IConfiguration config)
    {
        _config = config;
    }

    public async Task<List<string>> GetUserNotify(string pageId)
    {
        var channel = GrpcChannel.ForAddress(_config["GrpcUsersNotify"], new GrpcChannelOptions
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        var client = new NotifyUsers.NotifyUsersClient(channel);
        var request = new GetUsersRequest { PageId = pageId };
        try
        {
            var reply = await client.GetUsersAsync(request);
            return JsonSerializer.Deserialize<List<string>>(reply.Users);
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка GRPC - {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Невозможно вызвать GrpcUsersNotify сервер - {ex.Message}");
            return null;
        }
    }
}
