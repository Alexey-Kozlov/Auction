using Common.Contracts.Processing;

namespace Common.Contracts.Notification;


public enum MessageType
{
     Ошибка,
     Предупреждение,
     Сообщение
}

public class RequestEditNotification
{
     public Guid ItemId { get; set; }
     public string UserLogin { get; set; }
     public bool Enable { get; set; }
     public Guid CorrelationId { get; set; }
}

public class EditNotificationESCommit : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid ItemId { get; set; }
}

public class EditNotificationComplete : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid ItemId { get; set; }
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
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public Guid ItemId { get; set; }
}

public class EditNotificationEvent : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid ItemId { get; set; }
};

public class NotificationReset : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid ItemId { get; set; }
};

public class NotificationProgress
{
     public Guid CorrelationId { get; set; }
     public float Percent { get; set; }
     public int Duration { get; set; }
     public bool Show { get; set; }
     public string Message { get; set; }
     public string UserLogin { get; set; }
}

public class NotifyItem
{
     public Guid ItemId { get; set; }
     public string UserLogin { get; set; }
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
}