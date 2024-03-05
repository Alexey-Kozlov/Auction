using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using IdentityService.Data;
using IdentityService.Models;

namespace RedMangoShop.Controllers;

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
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerRequestDTO)
    {
        var response = new ApiResponse<object>();
        try
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(p => p.UserName.ToLower() == registerRequestDTO.Login.ToLower());
            if (user != null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.ErrorMessages.Add("Такой пользователь уже есть в БД");
                return BadRequest(response);
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
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            response.StatusCode = HttpStatusCode.BadRequest;
            response.IsSuccess = false;
            response.ErrorMessages.Add("Ошибка регистрации нового пользователя");
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.BadRequest;
            response.IsSuccess = false;
            response.ErrorMessages.Add("Ошибка регистрации нового пользователя - " + ex.Message);
            return BadRequest(response);
        }

    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
    {
        var response = new ApiResponse<LoginResponseDTO>();
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(p => p.Email.ToLower() == loginRequestDTO.Login.ToLower());
        if (user == null)
        {
            response.StatusCode = HttpStatusCode.BadRequest;
            response.IsSuccess = false;
            response.ErrorMessages.Add("Ошибка пользователя или пароля");
            return BadRequest(response);
        }
        var isValidUser = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
        if (!isValidUser)
        {
            response.StatusCode = HttpStatusCode.BadRequest;
            response.IsSuccess = false;
            response.ErrorMessages.Add("Ошибка пользователя или пароля");
            return BadRequest(response);
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
            response.StatusCode = HttpStatusCode.BadRequest;
            response.IsSuccess = false;
            response.ErrorMessages.Add("Ошибка пользователя или пароля");
            return BadRequest(response);
        }
        response.StatusCode = HttpStatusCode.OK;
        response.IsSuccess = true;
        response.Result = loginResponse;
        return Ok(response);
    }

}
