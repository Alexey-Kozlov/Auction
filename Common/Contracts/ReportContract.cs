namespace Common.Contracts.Report;

public record ReportParamsDTO(
    string Expression
);

public class SelectJson
{
    public string Label { get; set; }
    public string Value { get; set; }
    public bool Default { get; set; }
}

public class SqlQuery
{
    public string Text { get; set; }
    public List<string> Parameters { get; set; } = new();
}

public class DiagramData
{
    public string Seller { get; set; }
    public int ItemsCount { get; set; }
}