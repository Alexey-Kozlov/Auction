using System.Runtime.CompilerServices;
using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Communication;
using Common.Contracts.Report;
using CommunicationService.Data;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ReportService;

namespace CommunicationService.Services;

[Authorize]
public class GrpcReportService : GrpcReports.GrpcReportsBase
{
    private readonly CommunicationDbContext _dbContext;
    public GrpcReportService(CommunicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // получили от ReportService запрос для выборки комментариев для аукционов
    public override async Task<GrpcCommunicationReportResponse> GetCommunicationReport(GetCommunicationReportRequest request, ServerCallContext context)
    {
        var sqlQuery = JsonSerializer.Deserialize<SqlQuery>(request.CommunicationReportRequest);
        FormattableString formattedString;
        if (sqlQuery.Parameters.Count() > 0)
        {
            formattedString = FormattableStringFactory.Create(sqlQuery.Text, sqlQuery.Parameters.ToArray());
        }
        else
        {
            formattedString = FormattableStringFactory.Create(sqlQuery.Text);
        }
        var items = await _dbContext.Database.SqlQuery<CommunicationItem>(formattedString).ToListAsync();
        return new GrpcCommunicationReportResponse
        {
            CommunicationRezult = JsonSerializer.Serialize(new ApiResponse<List<CommunicationItem>>
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Result = items
            })
        };
    }
}
