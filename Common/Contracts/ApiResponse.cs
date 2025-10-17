using System.Net;

namespace Common.Contracts;

[Serializable]
public class ApiResponse<T>
{
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; } = true;
    public List<string> ErrorMessages { get; set; } = new();
    public T Result { get; set; }
}

[Serializable]
public class ApiErrorResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; } = true;
    public List<string> ErrorMessages { get; set; } = new();
}

[Serializable]
public class PagedResult<T>
{
    public T Results { get; set; }
    public int PageCount { get; set; }
    public int TotalCount { get; set; }
    public string UserLogin { get; set; }
    public int PageNumber { get; set; }
}