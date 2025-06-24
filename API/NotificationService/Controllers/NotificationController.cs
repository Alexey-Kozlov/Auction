using System.Linq.Expressions;
using Common.Contracts;
using Common.Contracts.Notification;
using Common.Contracts.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using Serialize.Linq.Serializers;

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
        var userNotify = await _context.NotifyItems.Where(p => p.Commited && p.ItemId == id &&
            p.UserLogin == userLogin).FirstOrDefaultAsync();
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

    [HttpPost("GetNotifyItemsByQuery")]
    public async Task<string> GetNotifyItemsByQuery(ReportParamsDTO dto)
    {
        var serializer = new ExpressionSerializer(new JsonSerializer())
        {
            AutoAddKnownTypesAsListTypes = true
        };
        var expression = serializer.DeserializeText(dto.Expression) as Expression<Func<NotifyItem, bool>>;
        var items = await _context.NotifyItems.Where(expression).ToListAsync();
        return System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<NotifyItem>>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = items
        });
    }
}
