using Common.Contracts;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;

namespace NotificationService.Services;

public class GetNotifyService
{
    private readonly NotificationDbContext _context;

    public GetNotifyService(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> IsNotifyUser(Guid id, string userLogin)
    {
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
}