namespace Scenarios.DTO;

public class LoginResponseDTO
{
    public string Login { get; set; }
    public string Name { get; set; }
    public string Token { get; set; }
    public bool IsGuest { get; set; }
}