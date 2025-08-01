using BiddingService.Data;
using Common.Contracts;
using Common.Contracts.Bid;
using Common.Contracts.Report;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ReportService;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BiddingService.Services;

public class GrpcReportService : GrpcReports.GrpcReportsBase
{
    private readonly BidDbContext _dbContext;
    public GrpcReportService(BidDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // получили от ReportService запрос для выборки данных по аукционам
    public override async Task<GrpcBidReportResponse> GetBidReport(GetBidReportRequest request, ServerCallContext context)
    {
        var sqlQuery = JsonSerializer.Deserialize<SqlQuery>(request.BidReportRequest);
        FormattableString formattedString;
        if (sqlQuery.Parameters.Count() > 0)
        {
            formattedString = FormattableStringFactory.Create(sqlQuery.Text, sqlQuery.Parameters.ToArray());
        }
        else
        {
            formattedString = FormattableStringFactory.Create(sqlQuery.Text);
        }
        var items = await _dbContext.Database.SqlQuery<BidItem>(formattedString).ToListAsync();
        return new GrpcBidReportResponse
        {
            BidRezult = new GrpcBidReportModel
            {
                BidItems = JsonSerializer.Serialize(new ApiResponse<List<BidItem>>
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Result = items
                })
            }
        };
    }
}
