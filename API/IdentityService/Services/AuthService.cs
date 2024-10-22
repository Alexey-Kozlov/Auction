using IdentityService.Models;
using Common.Utils;
using IdentityService.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

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
            UserName = registerRequestDTO.Name
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
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(p => p.Email.ToLower() == loginRequestDTO.Login.ToLower());
        if (user == null)
        {
            return new ApiResponse<LoginResponseDTO>()
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = ["Ошибка пользователя или пароля"],
                Result = new LoginResponseDTO()
            };
        }
        var isValidUser = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
        if (!isValidUser)
        {
            return new ApiResponse<LoginResponseDTO>()
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = ["Ошибка пользователя или пароля"],
                Result = new LoginResponseDTO()
            };
        }
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("Login", user.Email)
        };
        //захардкодили - роль админа только пользователю с именем "admin", остальным - роль "User"
        if (user.Email == "admin")
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, "User"));
        }

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            //получаем значение жизни токена из волта в виде json, используем свойство "expiration_hours"
            Expires = DateTime.UtcNow.AddHours(JsonDocument.Parse(_passwordPolicy).RootElement.GetProperty("expiration_hours").GetInt32()),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);

        var loginResponse = new LoginResponseDTO()
        {
            Name = user.UserName,
            Token = tokenHandler.WriteToken(token),
            Login = user.Email,
            Id = user.Id
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
}