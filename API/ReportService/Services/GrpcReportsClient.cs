using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Communication;
using Common.Contracts.Notification;
using Common.Contracts.Report;
using Grpc.Core;
using Grpc.Net.Client;

namespace ReportService.Services;

public class GrpcReportsClient
{
    private readonly IConfiguration _config;

    public GrpcReportsClient(IConfiguration config)
    {
        _config = config;
    }

    public async Task<ApiResponse<List<AuctionItem>>> GetAuctionReportItems(string auctionRequest)
    {
        var channel = GrpcChannel.ForAddress(_config["GrpcAuctionReport"], new GrpcChannelOptions
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        var client = new GrpcReports.GrpcReportsClient(channel);
        var request = new GetAuctionReportRequest { AuctionReportRequest = auctionRequest };

        try
        {
            var reply = await client.GetAuctionReportAsync(request);
            return JsonSerializer.Deserialize<ApiResponse<List<AuctionItem>>>(reply.AuctionRezult);
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка GRPC - {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Невозможно вызвать GrpcAuctionReport сервер - {ex.Message}");
            return null;
        }
    }

    public async Task<ApiResponse<List<BidItem>>> GetBidReportItems(string bidRequest)
    {
        var channel = GrpcChannel.ForAddress(_config["GrpcBidReport"], new GrpcChannelOptions
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        var client = new GrpcReports.GrpcReportsClient(channel);
        var request = new GetBidReportRequest { BidReportRequest = bidRequest };

        try
        {
            var reply = await client.GetBidReportAsync(request);
            return JsonSerializer.Deserialize<ApiResponse<List<BidItem>>>(reply.BidRezult);
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка GRPC - {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Невозможно вызвать GrpcBidReport сервер - {ex.Message}");
            return null;
        }
    }

    public async Task<ApiResponse<List<NotifyItem>>> GetNotificationReportItems(string notificationRequest)
    {
        var channel = GrpcChannel.ForAddress(_config["GrpcNotificationReport"], new GrpcChannelOptions
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        var client = new GrpcReports.GrpcReportsClient(channel);
        var request = new GetNotificationReportRequest { NotificationReportRequest = notificationRequest };

        try
        {
            var reply = await client.GetNotificationReportAsync(request);
            return JsonSerializer.Deserialize<ApiResponse<List<NotifyItem>>>(reply.NotificationRezult);
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка GRPC - {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Невозможно вызвать GrpcNotificationReport сервер - {ex.Message}");
            return null;
        }
    }

    public async Task<ApiResponse<List<DiagramData>>> GetDiagramReportItems(string diagramRequest)
    {
        var channel = GrpcChannel.ForAddress(_config["GrpcAuctionReport"], new GrpcChannelOptions
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        var client = new GrpcReports.GrpcReportsClient(channel);
        var request = new GetDiagramReportRequest { DiagramReportRequest = diagramRequest };

        try
        {
            var reply = await client.GetDiagramDataAsync(request);
            return JsonSerializer.Deserialize<ApiResponse<List<DiagramData>>>(reply.DiagramRezult);
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка GRPC - {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Невозможно вызвать GrpcDiagramReport сервер - {ex.Message}");
            return null;
        }
    }

    public async Task<ApiResponse<List<CommunicationItem>>> GetCommunicationReportItems(string communicationRequest)
    {
        var channel = GrpcChannel.ForAddress(_config["GrpcCommunicationReport"], new GrpcChannelOptions
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        var client = new GrpcReports.GrpcReportsClient(channel);
        var request = new GetCommunicationReportRequest { CommunicationReportRequest = communicationRequest };

        try
        {
            var reply = await client.GetCommunicationReportAsync(request);
            return JsonSerializer.Deserialize<ApiResponse<List<CommunicationItem>>>(reply.CommunicationRezult);
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка GRPC - {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Невозможно вызвать GrpcCommunicationReport сервер - {ex.Message}");
            return null;
        }
    }
}
