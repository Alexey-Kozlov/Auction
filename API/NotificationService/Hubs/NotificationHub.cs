using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Hubs;

public class NotificationHub : Hub
{

    public NotificationHub()
    {
    }
    public async override Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, Context.ConnectionId);
        var httpContext = Context.GetHttpContext();
        var userLogin = httpContext.User.FindFirst("Login").Value;
        await Groups.AddToGroupAsync(Context.ConnectionId, userLogin);

    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

}
