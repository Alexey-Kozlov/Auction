using System.Runtime.CompilerServices;
using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Report;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using TagService.Data;


namespace TagService.Services;

public class GrpcSearchService : GrpcSearch.GrpcSearchBase
{
    private readonly TagDbContext _dbContext;
    public GrpcSearchService(TagDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // получили от SearchService запрос для выборки аукционов по заданному тегу
    public override async Task<GrpcTagAuctionsResponse> GetTagAuctions(GetTagAuctionsRequest request, ServerCallContext context)
    {
        var sqlQuery = JsonSerializer.Deserialize<SqlQuery>(request.TagAuctionsRequest);
        FormattableString formattedString = FormattableStringFactory.Create(sqlQuery.Text, sqlQuery.Parameters.ToArray());

        var items = await _dbContext.Database.SqlQuery<Guid>(formattedString).ToListAsync();
        return new GrpcTagAuctionsResponse
        {
            Rezult = JsonSerializer.Serialize(new ApiResponse<List<Guid>>
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Result = items
            })
        };
    }
}
