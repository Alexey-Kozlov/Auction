namespace IdentityService.Models;

public record LoginResponseDTO
(
     string Name = "",
     string Token = "",
     string Login = "",
     bool IsGuest = false
);