using System.Text.Json;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class EditNotificationEventConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public EditNotificationEventConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<NotifyItem>> context)
    {
        var title = "";
        if (context.Message.DataObjects.Any())
        {
            foreach (var item in context.Message.DataObjects)
            {
                //уведомления при создании / удалении уведомления
                var typedItem = JsonSerializer.Deserialize<NotifyItem>(item.Data);
                title = context.Message.Props;
                switch (item.CRUD)
                {
                    case CRUD.Create:
                        title = $"Уведомление для пользователя {title} создано!";
                        break;
                    case CRUD.Delete:
                        //удаляем запись
                        title = $"Уведомление для пользователя {title} удалено!";
                        break;
                }
                await _hubContext.Clients.Group(typedItem.UserLogin).SendAsync("EditNotification",
                    new { message = title, show = true });
            }
        }
        else
        {
            await _hubContext.Clients.Group(context.Message.Props).SendAsync("EditNotification",
            new { show = Boolean.Parse(context.Message.Props), message = "" });
        }
    }
}
