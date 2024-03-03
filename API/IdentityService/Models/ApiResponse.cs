using System.Net;

namespace IdentityService.Models;

public class ApiResponse<T>
{
    public HttpStatusCode StatusCode {get; set;}
    public bool IsSuccess {get; set;} = true;
    public List<string> ErrorMessages {get; set;} = new();
    public T Result {get; set;}
}