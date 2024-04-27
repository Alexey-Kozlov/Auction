using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;

namespace NotificationService.Hubs;

public class NotificationHub : Hub
{
    private readonly NotificationDbContext _dbContext;

    public NotificationHub(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async override Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var userLogin = httpContext.User.FindFirst("Login").Value;
        await Groups.AddToGroupAsync(Context.ConnectionId, userLogin);
    }
}
