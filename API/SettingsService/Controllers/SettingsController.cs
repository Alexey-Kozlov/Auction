using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SettingsService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    public SettingsController(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }


    [HttpPost("setuserscurrentpage")]
    public void SetUserCurrentPage([FromBody] string fake)
    {
        //заглушка, смысл - на этот ендпойнт приходит служебный запрос по записи текущей страницы 
        // пользователя, сама запись осуществляется в GatewayService, где обрабатываются логи
    }
}