using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Common.Utils;
using NotificationService.Data;
using NotificationService.DTO;
using NotificationService.Entities;

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
        var userNotify = await _context.NotifyUser.Where(p => p.AuctionId == id && p.UserLogin == userLogin).FirstOrDefaultAsync();
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

    [Authorize]
    [HttpPost]
    public async Task<ApiResponse<bool>> SetNotifyUser([FromBody] NotifyUserDTO notifyUserDTO)
    {
        var userLogin = User.FindFirst("Login").Value;
        var userNotify = await _context.NotifyUser.Where(p => p.AuctionId == notifyUserDTO.Id &&
            p.UserLogin == userLogin).FirstOrDefaultAsync();
        if (userNotify != null && !notifyUserDTO.Enable)
        {
            _context.Remove(userNotify);
        }
        if (userNotify == null && notifyUserDTO.Enable)
        {
            _context.NotifyUser.Add(new NotifyUser { AuctionId = notifyUserDTO.Id, UserLogin = userLogin });
        }

        await _context.SaveChangesAsync();
        return new ApiResponse<bool>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = true
        };
    }

}
