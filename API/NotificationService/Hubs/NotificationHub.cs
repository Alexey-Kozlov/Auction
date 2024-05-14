using Microsoft.AspNetCore.SignalR;
using NotificationService.Data;

namespace NotificationService.Hubs;

public class NotificationHub : Hub
{

    public NotificationHub()
    {
    }
    public async override Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var userLogin = httpContext.User.FindFirst("Login").Value;
        await Groups.AddToGroupAsync(Context.ConnectionId, userLogin);
    }
}
