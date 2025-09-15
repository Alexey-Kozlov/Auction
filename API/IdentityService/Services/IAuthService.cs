using Common.Contracts;
using IdentityService.Models;

namespace IdentityService.Services;

public interface IAuthService
{
    Task<ApiResponse<object>> Register(RegisterRequestDTO registerRequestDTO);
    Task<ApiResponse<LoginResponseDTO>> Login(LoginRequestDTO loginRequestDTO);
    Task<ApiResponse<string>> GetUserName(GetUserNameDTO dto);
    Task<ApiResponse<object>> SetPassword(LoginRequestDTO dto);
    Task<ApiResponse<LoginResponseDTO>> SetRefreshToken(string userLogin);
}

