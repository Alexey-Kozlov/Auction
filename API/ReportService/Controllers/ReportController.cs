using System.Linq.Expressions;
using Common.Contracts.Report;
using Microsoft.AspNetCore.Mvc;
using ReportService.DTO;
using ReportService.Services;

namespace ReportService.Controllers;

[ApiController]
[Route("api/report")]
public class ReportController : ControllerBase
{
    private readonly GetDataService _reportService;

    public ReportController(GetDataService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost("auctionlist")]
    public async Task<string> AuctionList([FromBody] ParamItem[] param)
    {
        return await _reportService.GetAuctionListData(param);
    }

}

public class Helper<T, TResult>
{

    public Type Type { get; set; }
    public string Method { get; set; }
    public Type[] ArgTypes { get; set; }
    public object[] ArgValues { get; set; }

    public Helper(Expression<Func<T, TResult>> expression)
    {
        var body = (MethodCallExpression)expression.Body;

        Type = typeof(T);
        Method = body.Method.Name;
        ArgTypes = body.Arguments.Select(x => x.Type).ToArray();
        var values = new List<object>();
        foreach (var arg in body.Arguments)
        {
            var value = Expression.Lambda(arg).Compile().DynamicInvoke();
            values.Add(value);
        }
        this.ArgValues = values.ToArray();
    }
}