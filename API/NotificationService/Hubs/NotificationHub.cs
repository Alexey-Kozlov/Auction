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
        if (httpContext.User.Identity.IsAuthenticated)
        {
            var userLogin = httpContext.User.FindFirst("Login").Value;
            await Groups.AddToGroupAsync(Context.ConnectionId, userLogin);
        }
        //посылаем клиенту SessionId для последующей идентификации при персональной рассылке
        await Clients.Caller.SendAsync("SessionId", Context.ConnectionId);
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.ConnectionId);
        var httpContext = Context.GetHttpContext();
        if (httpContext.User.Identity.IsAuthenticated)
        {
            var userLogin = httpContext.User.FindFirst("Login").Value;
            Groups.RemoveFromGroupAsync(Context.ConnectionId, userLogin);
        }
        return base.OnDisconnectedAsync(exception);
    }

}
