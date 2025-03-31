using Common.Contracts.Processing;

namespace Common.Contracts.Notification;

public class NotifyItem
{
     public Guid? Id { get; set; }
     public Guid AuctionId { get; set; }
     public string UserLogin { get; set; }
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
}



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

public class EditNotificationCreated : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string Message { get; set; }
     public string ExceptionMessage { get; set; }
     public string UserLogin { get; set; }
     public string ServiceName { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
}

public class EditNotificationESCommit : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string Message { get; set; }
     public string ExceptionMessage { get; set; }
     public string UserLogin { get; set; }
     public string ServiceName { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
}

public class EditNotificationComplete : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string Message { get; set; }
     public string ExceptionMessage { get; set; }
     public string UserLogin { get; set; }
     public string ServiceName { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
}

public class AuctionNotificationData
{
     public Guid CorrelationId { get; set; }
     public string AuctionData { get; set; }
     public CRUD CRUD { get; set; }
}

public class NotificationCommit
{
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
     public string CallBackType { get; set; }
}

public class EditNotificationEvent : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string Message { get; set; }
     public string ExceptionMessage { get; set; }
     public string UserLogin { get; set; }
     public string ServiceName { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
};
