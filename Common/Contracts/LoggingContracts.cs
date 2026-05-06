namespace Common.Contracts.Logging;

public class RequestLoggingContract
{
    public string Method { get; set; }
    public string Path { get; set; }
    public string Body { get; set; }
}

public class ResponseLoggingContract : ApiResponse<string>
{
    public string InputData { get; set; }
    public string ServiceName { get; set; }
}

public class ItemLoggingContract
{
    public string TraceId { get; set; }
    public string RequestType { get; set; }
    public string UserLogin { get; set; }
    public DateTime RequestDate { get; set; }
    public RequestLoggingContract RequestLoggingContract { get; set; }
    public ResponseLoggingContract ResponseLoggingContract { get; set; }
    public LogType LogType { get; set; }
}

public enum LogType
{
    Audit,
    Error,
    System,
    Trace
}
