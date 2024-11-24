using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Common.Utils;
using NotificationService.Data;

namespace NotificationService.Controllers;

[ApiController]
[Route("notifications/items")]
public class NotificationController : ControllerBase
{
    private readonly NotificationDbContext _context;

    public NotificationController(NotificationDbContext context)
    {
        _context = context;
    }


    [Authorize]
    [HttpGet("{id}")]
    public async Task<ApiResponse<bool>> IsNotifyUser(Guid id)
    {
        var userLogin = User.FindFirst("Login").Value;
        var userNotify = await _context.NotifyItems.Where(p => p.AuctionId == id && p.UserLogin == userLogin).FirstOrDefaultAsync();
        var rezult = new ApiResponse<bool>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = true
        };
        if (userNotify == null)
        {
            rezult.Result = false;
        }
        return rezult;
    }

}
