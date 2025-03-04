namespace Common.Contracts;

public class RequestLoggingContract
{
    public string Method { get; set; }
    public string Path { get; set; }
    public string Body { get; set; }
}

public class ResponseLoggingContract
{
    public string Body { get; set; }
    public int StatusCode { get; set; }
}

public class ItemLoggingContract
{
    public string RequestId { get; set; }
    public string UserLogin { get; set; }
    public DateTime RequestDate { get; set; }
    public string Roles { get; set; }
    public RequestLoggingContract RequestLoggingContract { get; set; }
    public ResponseLoggingContract ResponseLoggingContract { get; set; }
}
