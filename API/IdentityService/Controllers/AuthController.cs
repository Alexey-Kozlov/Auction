using Microsoft.AspNetCore.Mvc;
using IdentityService.Models;
using Common.Utils;
using IdentityService.Services;

namespace IdentityService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{

    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Register")]
    public async Task<ApiResponse<object>> Register([FromBody] RegisterRequestDTO registerRequestDTO)
    {
        return await _authService.Register(registerRequestDTO);
    }

    [HttpPost("Login")]
    public async Task<ApiResponse<LoginResponseDTO>> Login([FromBody] LoginRequestDTO loginRequestDTO)
    {
        return await _authService.Login(loginRequestDTO);
    }

    [HttpPost]
    public async Task<ApiResponse<string>> GetUserName([FromBody] GetUserNameDTO dto)
    {
        return await _authService.GetUserName(dto);
    }

    [HttpPost("SetNewPassword")]
    public async Task<ApiResponse<object>> SetPassword([FromBody] LoginRequestDTO dto)
    {
        return await _authService.SetPassword(dto);
    }

}
