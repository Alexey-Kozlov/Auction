using System.Runtime.CompilerServices;
using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.Report;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ReportService;
using SearchService.Data;

namespace SearchService.Services;

public class GrpcReportService : GrpcReports.GrpcReportsBase
{
    private readonly SearchDbContext _dbContext;
    public GrpcReportService(SearchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // получили от ReportService запрос для выборки данных по аукционам
    public override async Task<GrpcAuctionReportResponse> GetAuctionReport(GetAuctionReportRequest request, ServerCallContext context)
    {
        var sqlQuery = JsonSerializer.Deserialize<SqlQuery>(request.AuctionReportRequest);
        FormattableString formattedString;
        if (sqlQuery.Parameters.Count() > 0)
        {
            formattedString = FormattableStringFactory.Create(sqlQuery.Text, sqlQuery.Parameters.ToArray());
        }
        else
        {
            formattedString = FormattableStringFactory.Create(sqlQuery.Text);
        }
        var items = await _dbContext.Database.SqlQuery<AuctionItem>(formattedString).ToListAsync();
        return new GrpcAuctionReportResponse
        {
            AuctionRezult = new GrpcAuctionReportModel
            {
                AuctionItems = JsonSerializer.Serialize(new ApiResponse<List<AuctionItem>>
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Result = items
                })
            }
        };
    }
}
