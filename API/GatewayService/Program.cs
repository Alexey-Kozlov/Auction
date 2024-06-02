using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Common.Utils;
using GatewayService.Services;
using GatewayService.Cache;
using GatewayService.Models;
using MassTransit;
using GatewayService.Consumers;
using Microsoft.AspNetCore.Http.HttpResults;

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

builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<AuctionUpdatingGatewayConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("gateway", false));
    p.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["RabbitMq:Host"], "/", p =>
        {
            p.Username(builder.Configuration.GetValue("RabbitMq:UserName", "guest"));
            p.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        config.ConfigureEndpoints(context);
    });
});

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

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Config"];
    options.InstanceName = builder.Configuration["Redis:Instance"];
});

builder.Services.AddScoped<GrpcImageClient>();
builder.Services.AddScoped<ImageCache>();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("customPolicy");

//получаем изображения из кеша - сервис с использованием reddis
app.MapGet("/api/images/{id}", async (string id, ImageCache imageCache) =>
{
    var img = await imageCache.GetImage(id);
    return new ApiResponse<ImageDTO>()
    {
        StatusCode = System.Net.HttpStatusCode.OK,
        IsSuccess = true,
        Result = new ImageDTO(id, img)
    };
});
app.MapGet("/api/images_dop/{id}", async (string id, ImageCache imageCache) =>
{
    return "data:image/png;base64, " + await imageCache.GetImage(id);
});
app.MapGet("/api/images_file/{id}", async (string id, ImageCache imageCache) =>
{
    var img = await imageCache.GetImage(id);
    return Results.File(Convert.FromBase64String(img), contentType: "image/png");
});

app.MapReverseProxy();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
