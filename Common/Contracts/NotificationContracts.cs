namespace Common.Contracts.Notification;

public class NotifyItem
{
     public Guid AuctionId { get; set; }
     public string UserLogin { get; set; }
}
public record UserNotificationSet(
     Guid AuctionId,
     string UserLogin,
     Guid CorrelationId
);

public record UserNotificationAdded(Guid CorrelationId);

public record FaultNotificationMessage(
     string UserLogin,
     string Message,
     MessageType MessageType,
     Guid? AuctionId);

public enum MessageType
{
     Ошибка,
     Предупреждение,
     Сообщение
}

public class RequestEditNotification
{
     public Guid AuctionId { get; set; }
     public string UserLogin { get; set; }
     public bool Enable { get; set; }
     public string SessionId { get; set; }
     public Guid CorrelationId { get; set; }
}

public class EditNotificationCreated
{
     public Guid CorrelationId { get; set; }
}

public class EditNotificationESCommit
{
     public Guid CorrelationId { get; set; }
}
public class EditNotificationComplete
{
     public Guid CorrelationId { get; set; }
}