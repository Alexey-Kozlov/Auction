namespace IdentityService.Models;

public record RegisterRequestDTO
(
     string Login,
     string Name,
     string Password
);