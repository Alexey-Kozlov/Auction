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