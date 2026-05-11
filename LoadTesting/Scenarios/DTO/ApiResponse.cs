using System.Net;
namespace Scenarios.DTO;

public class ApiResponse<T>
{
    public HttpStatusCode StatusCode { get; set; }

    public bool IsSuccess { get; set; } = true;

    public List<string> ErrorMessages { get; set; } = new List<string>();

    public T Result { get; set; }
}

public class LoginResponseDTO
{
    public string Login { get; set; }
    public string Name { get; set; }
    public string Token { get; set; }
    public bool IsGuest { get; set; }
}