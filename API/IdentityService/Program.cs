using System.Text;
using IdentityService;
using IdentityService.Data;
using Common.Utils;
using IdentityService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Polly;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequiredLength = 1;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮйцукенгшщзхъфывапролджэячсмитьбю";
});

//конфигурация конвейера для работы с JWT-аутентификацией
builder.Services.AddAuthentication(p =>
{
    p.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    p.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(p =>
{
    p.RequireHttpsMetadata = false;
    p.SaveToken = true;
    p.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("ApiSettings:Secret"))),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
builder.Services.AddControllers();
builder.Services.AddCors();
var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().WithExposedHeaders("*"));
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var retryPolicy = Policy
.Handle<NpgsqlException>()
.WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(10));
retryPolicy.ExecuteAndCapture(() => SeedData.EnsureSeedData(app));


app.Run();