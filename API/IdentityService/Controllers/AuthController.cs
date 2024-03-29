using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using IdentityService.Data;
using IdentityService.Models;

namespace IdentityService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{

    private readonly ApplicationDbContext _db;
    private readonly string _secretKey;
    private readonly UserManager<ApplicationUser> _userManager;
    public AuthController(ApplicationDbContext db, IConfiguration configuration,
    UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        _userManager = userManager;
    }

    [HttpPost("Register")]
    public async Task<ApiResponse<object>> Register([FromBody] RegisterRequestDTO registerRequestDTO)
    {
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(p => p.UserName.ToLower() == registerRequestDTO.Login.ToLower());
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

    [HttpPost("Login")]
    public async Task<ApiResponse<LoginResponseDTO>> Login([FromBody] LoginRequestDTO loginRequestDTO)
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
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim("Name", user.UserName),
                new Claim("Login", user.Email),
            }),
            Expires = DateTime.UtcNow.AddDays(1),
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

}
