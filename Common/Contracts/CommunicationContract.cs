using Common.Contracts.Processing;

namespace Common.Contracts.Communication;

public class CommunicationItem
{
    public Guid? ItemId { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? AuctionId { get; set; }
    public string UserLogin { get; set; }
    public string Message { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdateAt { get; set; }
    public Guid CorrelationId { get; set; }
    public bool Commited { get; set; }
}

public class RequestCommunicationCreate
{
    public Guid? ParentId { get; set; }
    public Guid? ItemId { get; set; }
    public Guid AuctionId { get; set; }
    public string UserLogin { get; set; }
    public string Message { get; set; }
    public Guid CorrelationId { get; set; }
    public string SessionId { get; set; }
}

public class RequestCommunicationUpdate
{
    public Guid ItemId { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? ParentId { get; set; }
    public string UserLogin { get; set; }
    public string Message { get; set; }
    public Guid CorrelationId { get; set; }
    public string SessionId { get; set; }
}

public class RequestCommunicationDelete
{
    public Guid ItemId { get; set; }
    public Guid? AuctionId { get; set; }
    public string UserLogin { get; set; }
    public Guid CorrelationId { get; set; }
    public string SessionId { get; set; }
}

public class CommunicationCreateSearch : IFaultMessage
{
    public Guid CorrelationId { get; set; }

    public string UserLogin { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorExceptionMessage { get; set; }

    public string ErrorServiceName { get; set; }

    public string CallBackType { get; set; }

    public Guid? AuctionId { get; set; }

    public bool IsError { get; set; }
}

public class CommunicationDeleteSearch : IFaultMessage
{
    public Guid CorrelationId { get; set; }

    public string UserLogin { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorExceptionMessage { get; set; }

    public string ErrorServiceName { get; set; }

    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }

    public Guid ItemId { get; set; }

    public bool IsError { get; set; }
}

public class CommunicationUpdateSearch : IFaultMessage
{
    public Guid CorrelationId { get; set; }

    public string UserLogin { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorExceptionMessage { get; set; }

    public string ErrorServiceName { get; set; }

    public string CallBackType { get; set; }

    public Guid ItemId { get; set; }
    public Guid? AuctionId { get; set; }

    public bool IsError { get; set; }
}

public class CommunicationCreateNotificationEvent : IFaultMessage
{
    public Guid CorrelationId { get; set; }

    public string UserLogin { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorExceptionMessage { get; set; }

    public string ErrorServiceName { get; set; }

    public string CallBackType { get; set; }

    public Guid? AuctionId { get; set; }

    public bool IsError { get; set; }
}

public class CommunicationDeleteNotificationEvent : IFaultMessage
{
    public Guid CorrelationId { get; set; }

    public string UserLogin { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorExceptionMessage { get; set; }

    public string ErrorServiceName { get; set; }

    public string CallBackType { get; set; }

    public Guid ItemId { get; set; }
    public Guid? AuctionId { get; set; }

    public bool IsError { get; set; }
}

public class CommunicationUpdateNotificationEvent : IFaultMessage
{
    public Guid CorrelationId { get; set; }

    public string UserLogin { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorExceptionMessage { get; set; }

    public string ErrorServiceName { get; set; }

    public string CallBackType { get; set; }

    public Guid ItemId { get; set; }
    public Guid? AuctionId { get; set; }

    public bool IsError { get; set; }
}

public class CommunicationCreateESCommit : IFaultMessage
{
    public Guid CorrelationId { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorExceptionMessage { get; set; }

    public string ErrorServiceName { get; set; }

    public string UserLogin { get; set; }

    public string CallBackType { get; set; }

    public Guid? AuctionId { get; set; }

    public bool IsError { get; set; }
}

public class CommunicationDeleteESCommit : IFaultMessage
{
    public Guid CorrelationId { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorExceptionMessage { get; set; }

    public string ErrorServiceName { get; set; }

    public string UserLogin { get; set; }

    public string CallBackType { get; set; }

    public Guid AuctionId { get; set; }

    public bool IsError { get; set; }
}

public class CommunicationUpdateESCommit : IFaultMessage
{
    public Guid CorrelationId { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorExceptionMessage { get; set; }

    public string ErrorServiceName { get; set; }

    public string UserLogin { get; set; }

    public string CallBackType { get; set; }

    public Guid ItemId { get; set; }
    public Guid? AuctionId { get; set; }

    public bool IsError { get; set; }
}

public class CommunicationSearch
{
    public Guid? ItemId { get; set; }
    public Guid AuctionId { get; set; }
    public string Message { get; set; }
}

public class CommunicationCommit
{
    public Guid CorrelationId { get; set; }

    public bool Commited { get; set; }

    public string CallBackType { get; set; }

    public string UserLogin { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorExceptionMessage { get; set; }

    public string ErrorServiceName { get; set; }
}