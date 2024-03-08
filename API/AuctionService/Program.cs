using System.Text;
using AuctionService.Consumers;
using AuctionService.Data;
using AuctionService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(p => 
{
    p.AddEntityFrameworkOutbox<AuctionDbContext>(config =>
    {
        config.QueryDelay = TimeSpan.FromSeconds(10);
        config.UsePostgres();
        config.UseBusOutbox();
    });
    p.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
    p.UsingRabbitMq((context, config) => 
    {
        config.Host(builder.Configuration["RabbitMq:Host"], "/", p =>
        {
            p.Username(builder.Configuration.GetValue("RabbitMq:UserName","guest"));
            p.Password(builder.Configuration.GetValue("RabbitMq:Password","guest"));
        });
        config.ConfigureEndpoints(context);
    });
});

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
    ValidateAudience = false,
    NameClaimType="Login"
   };
});
builder.Services.AddGrpc();

builder.Services.AddSwaggerGen(opt =>
{
    //здесь конфиг для сваггера - чтобы можно было проверять JWT-аутентификацию
    opt.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
    new OpenApiSecurityScheme
    {
        Description = "JWT Authorization" +
        "Введите 'Bearer' [space] and token, например -> \"Bearer 12345kjhgkhgkj\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GrpcAuctionService>();

try
{
    DbInit.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine("Ошибка инициализации БД Auction - " + e.Message);
}

app.Run();
