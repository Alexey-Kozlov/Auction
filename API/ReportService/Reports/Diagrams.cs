using System.Text.Json;
using Common.Contracts.Report;
using ReportService.DTO;
using ReportService.Services;

namespace ReportService.Reports;

public class Diagrams
{
    private readonly GrpcReportsClient _client;
    public Diagrams(GrpcReportsClient client)
    {
        _client = client;
    }

    public Task<string> GetDiagrams(ParamItem[] param)
    {
        var BeginDatePar = param.FirstOrDefault(p => p.Id == "BeginDate").Value;
        var EndDatePar = param.FirstOrDefault(p => p.Id == "EndDate").Value;
        var query = new SqlQuery();
        //получаем статистику по аукционам за заданный период
        query.Text = "select \"Seller\",count(*) as \"ItemsCount\" from \"SearchItems\" where true";
        if (!string.IsNullOrEmpty(BeginDatePar) && !string.IsNullOrEmpty(EndDatePar))
        {
            query.Text += " and \"CreateAt\" between {0} and {1}";
            query.Parameters.Add(BeginDatePar);
            query.Parameters.Add(EndDatePar);
        }
        query.Text += " group by \"Seller\"";
        List<DiagramData> diagramData = _client.GetDiagramReportItems(JsonSerializer.Serialize(query))
             .GetAwaiter().GetResult().Result;
        if (diagramData.Count() == 0) return Task.FromResult("[]");
        var resultDiagram = diagramData.Select(p => new
        {
            Seller = p.Seller,
            ItemsCount = p.ItemsCount
        });
        return Task.FromResult(JsonSerializer.Serialize(resultDiagram));
    }
}