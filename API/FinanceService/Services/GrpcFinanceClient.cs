using Grpc.Core;
using Grpc.Net.Client;

namespace FinanceService.Services;

public class GrpcFinanceClient
{
    private readonly IConfiguration _config;

    public GrpcFinanceClient(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> GetFinance(string financeSortRequest)
    {
        var channel = GrpcChannel.ForAddress(_config["GrpcFinance"], new GrpcChannelOptions
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        var client = new GrpcFinance.GrpcFinanceClient(channel);
        var request = new GetFinanceRequest { FinanceSortRequest = financeSortRequest };

        try
        {
            var reply = await client.GetFinanceAsync(request);
            var rez = reply.FinancePagedRezult;
            return rez.FinancePagedRezult;
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка GRPC Finance - {ex.Message}");
            return "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Невозможно вызвать GRPC Finance сервер - {ex.Message}");
            return "";
        }
    }
}
