using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
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
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.Authority = builder.Configuration["IdentityServiceUrl"];
//         options.RequireHttpsMetadata = false;
//         options.TokenValidationParameters.ValidateAudience = false;
//         options.TokenValidationParameters.NameClaimType = "login";
//     });

builder.Services.AddCors(options =>
{
    options.AddPolicy("customPolicy", p =>
    {
        p.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(builder.Configuration["ClientApp"]);
    });
});

var app = builder.Build();

app.UseCors("customPolicy");

app.MapReverseProxy();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
