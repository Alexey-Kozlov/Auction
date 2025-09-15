using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Common.Contracts;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly string _secretKey;
    private readonly string _passwordPolicy;

    public AuthService(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _db = db;
        _secretKey = configuration["api:secret"];
        _userManager = userManager;
        _passwordPolicy = configuration["pw:password_policy"];
    }
    public async Task<ApiResponse<object>> Register(RegisterRequestDTO registerRequestDTO)
    {
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(p => p.Email.ToLower() == registerRequestDTO.Login.ToLower());
        if (user != null)
        {
            return new ApiResponse<object>()
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = ["Такой пользователь уже есть в БД"],
                Result = null
            };
        }

        var newUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = registerRequestDTO.Login,
            UserName = registerRequestDTO.Name,
        };

        var result = await _userManager.CreateAsync(newUser, registerRequestDTO.Password);
        if (result.Succeeded)
        {
            return new ApiResponse<object>()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = new { data = "Ok" }
            };
        }
        throw new Exception("Ошибка регистрации нового пользователя - " + result.Errors.First().Description);
    }

    public async Task<ApiResponse<LoginResponseDTO>> Login(LoginRequestDTO loginRequestDTO)
    {
        if (loginRequestDTO.IsGuest)
        {
            return GuestLogin(loginRequestDTO);
        }
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(p => p.Email.ToLower() == loginRequestDTO.Login.ToLower());
        if (user == null)
        {
            return new ApiResponse<LoginResponseDTO>()
            {
                StatusCode = HttpStatusCode.Forbidden,
                IsSuccess = false,
                ErrorMessages = [$"Ошибка пользователя или пароля, пользователь - '{loginRequestDTO.Login}'"],
                Result = new LoginResponseDTO()
            };
        }
        var isValidUser = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
        if (!isValidUser)
        {
            return new ApiResponse<LoginResponseDTO>()
            {
                StatusCode = HttpStatusCode.Forbidden,
                IsSuccess = false,
                ErrorMessages = [$"Ошибка пользователя или пароля, пользователь - '{loginRequestDTO.Login}'"],
                Result = new LoginResponseDTO(),
            };
        }
        var tokenHandler = new JwtSecurityTokenHandler();
        var loginResponse = new LoginResponseDTO()
        {
            Name = user.UserName,
            Token = tokenHandler.WriteToken(GenerateToken(user.Email, false,
                user.Email == "admin" ? "Admin" : "User")),
            Login = user.Email,
            IsGuest = false
        };
        if (string.IsNullOrEmpty(loginResponse.Token))
        {
            return new ApiResponse<LoginResponseDTO>()
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = ["Ошибка пользователя или пароля"],
                Result = new LoginResponseDTO()
            };
        }
        return new ApiResponse<LoginResponseDTO>()
        {
            StatusCode = HttpStatusCode.OK,
            IsSuccess = true,
            Result = loginResponse
        };
    }

    public async Task<ApiResponse<string>> GetUserName(GetUserNameDTO dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.login);
        return new ApiResponse<string>()
        {
            StatusCode = HttpStatusCode.OK,
            IsSuccess = true,
            Result = user == null ? "" : user.UserName
        };
    }

    public async Task<ApiResponse<object>> SetPassword(LoginRequestDTO setPasswordDTO)
    {
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(p => p.Email.ToLower() == setPasswordDTO.Login.ToLower());
        if (user == null)
        {
            return new ApiResponse<object>()
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = [$"Пользователь {setPasswordDTO.Login} не найден"],
                Result = null
            };
        }

        var result = await _userManager.RemovePasswordAsync(user);
        if (result.Succeeded)
        {
            result = await _userManager.AddPasswordAsync(user, setPasswordDTO.Password);
            if (result.Succeeded)
            {
                return new ApiResponse<object>()
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Result = new { data = "Ok" }
                };
            }
        }

        throw new Exception("Ошибка регистрации нового пользователя - " + result.Errors.First().Description);
    }

    private ApiResponse<LoginResponseDTO> GuestLogin(LoginRequestDTO loginRequestDTO)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var loginResponse = new LoginResponseDTO()
        {
            Name = loginRequestDTO.Login,
            Token = tokenHandler.WriteToken(GenerateToken(loginRequestDTO.Login, true, "User")),
            Login = loginRequestDTO.Login,
            IsGuest = true,
        };
        if (string.IsNullOrEmpty(loginResponse.Token))
        {
            return new ApiResponse<LoginResponseDTO>()
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = ["Ошибка пользователя или пароля"],
                Result = new LoginResponseDTO()
            };
        }
        return new ApiResponse<LoginResponseDTO>()
        {
            StatusCode = HttpStatusCode.OK,
            IsSuccess = true,
            Result = loginResponse
        };
    }

    private SecurityToken GenerateToken(string userLogin, bool isGuest, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userLogin),
            new Claim("Login", userLogin),
            new Claim("IsGuest",isGuest.ToString().ToLower()),
            new Claim(ClaimTypes.Role, role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            //получаем значение жизни токена из волта в виде json, используем свойство "expiration_minutes"
            Expires = DateTime.UtcNow.AddMinutes(JsonDocument.Parse(_passwordPolicy).RootElement.GetProperty("expiration_minutes").GetInt32()),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        return tokenHandler.CreateToken(tokenDescriptor);
    }

    public async Task<ApiResponse<LoginResponseDTO>> SetRefreshToken(string userLogin)
    {
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(p => p.Email.ToLower() == userLogin.ToLower());
        var tokenHandler = new JwtSecurityTokenHandler();
        var loginResponse = new LoginResponseDTO()
        {
            Name = user == null ? userLogin : user.UserName,
            Token = tokenHandler.WriteToken(GenerateToken(userLogin,
                user == null ? true : false,
                userLogin == "admin" ? "Admin" : "User")),
            Login = userLogin,
            IsGuest = user == null ? true : false
        };
        return new ApiResponse<LoginResponseDTO>()
        {
            StatusCode = HttpStatusCode.OK,
            IsSuccess = true,
            Result = loginResponse
        };

    }
}