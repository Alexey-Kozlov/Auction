using Common.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("notifications/items")]
public class NotificationController : ControllerBase
{
    private readonly GetNotifyService _getNotifyService;

    public NotificationController(GetNotifyService getNotifyService)
    {
        _getNotifyService = getNotifyService;
    }


    [Authorize]
    [HttpGet("{id}")]
    public async Task<ApiResponse<bool>> IsNotifyUser(Guid id)
    {
        var userLogin = User.FindFirst("Login").Value;

        return await _getNotifyService.IsNotifyUser(id, userLogin);
    }
}
