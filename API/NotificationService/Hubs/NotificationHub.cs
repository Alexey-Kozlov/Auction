using Common.Contracts.Communication;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.DTO;

namespace NotificationService.Hubs;

public class NotificationHub : Hub
{
    private readonly IPublishEndpoint _publishEndpoint;

    public NotificationHub(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
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

    public async Task SendComment(MessageChat comment)
    {
        switch (comment.ActionType)
        {
            case ActionType.create:
                //приняли от фронта создание нового комментария к аукциону. Запускаем процесс создания комментария
                await _publishEndpoint.Publish(new RequestCommunicationCreate
                {
                    ItemId = Guid.Parse(comment.ItemId),
                    AuctionId = Guid.Parse(comment.AuctionId),
                    CorrelationId = Guid.NewGuid(),
                    Message = comment.Message,
                    ParentId = string.IsNullOrEmpty(comment.ParentId) ? null : Guid.Parse(comment.ParentId),
                    UserLogin = comment.UserLogin,
                });
                break;
            case ActionType.delete:
                //удаление комментария к аукциону. Запускаем процесс удаления
                await _publishEndpoint.Publish(new RequestCommunicationDelete
                {
                    ItemId = Guid.Parse(comment.ItemId),
                    AuctionId = Guid.Parse(comment.AuctionId),
                    CorrelationId = Guid.NewGuid(),
                    UserLogin = comment.UserLogin,
                });
                break;
            case ActionType.update:
                //редактирование комментария к аукциону. Запускаем процесс редактирования
                await _publishEndpoint.Publish(new RequestCommunicationUpdate
                {
                    ItemId = Guid.Parse(comment.ItemId),
                    AuctionId = Guid.Parse(comment.AuctionId),
                    Message = comment.Message,
                    CorrelationId = Guid.NewGuid(),
                    UserLogin = comment.UserLogin,
                });
                break;
        }
    }
}
