using Common.Contracts.Processing;

namespace Common.Contracts.Tag;

public class TagItem
{
     public Guid AuctionId { get; set; }
     public Guid ItemId { get; set; }
     public string Name { get; set; }
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
}

public class TagList
{
     public string Name { get; set; }
}

public class RequestCreateTag
{
     public Guid AuctionId { get; set; }
     public string UserLogin { get; set; }
     public Guid CorrelationId { get; set; }
     public string Name { get; set; }
}

public class RequestDeleteTag
{
     public Guid AuctionId { get; set; }
     public string UserLogin { get; set; }
     public Guid CorrelationId { get; set; }
     public string Name { get; set; }
}

public class ModifyTag
{
     public string UserLogin { get; set; }
     public Guid CorrelationId { get; set; }
     public Guid AuctionId { get; set; }
     public string CallBackType { get; set; }
     public string Name { get; set; }
     public bool Commited { get; set; }
     public Guid ItemId { get; set; }
}

public class AddTagCompleted : IFaultMessage
{
     public Guid CorrelationId { get; set; }

     public string UserLogin { get; set; }

     public string CallBackType { get; set; }

     public bool IsError { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
}

public class DeleteTagCompleted : IFaultMessage
{
     public Guid CorrelationId { get; set; }

     public string UserLogin { get; set; }

     public string CallBackType { get; set; }

     public bool IsError { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
}

public class TagCreateESCommit : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public Guid ItemId { get; set; }
     public bool IsError { get; set; }
}

public class TagCreateNotificationEvent : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public string Data { get; set; }
     public bool IsError { get; set; }
}

public class TagDeleteESCommit : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public Guid ItemId { get; set; }
     public bool IsError { get; set; }
}

public class TagDeleteNotificationEvent : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public string Data { get; set; }
     public bool IsError { get; set; }
}

public class TagCommit
{
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
     public string CallBackType { get; set; }
     public string UserLogin { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
}

public class TagReset : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid ItemId { get; set; }
     public bool IsError { get; set; }
}

public class TagListRequest : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public Guid ItemId { get; set; }
     public bool IsError { get; set; }
}

public class TagListCommit : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public Guid ItemId { get; set; }
     public bool IsError { get; set; }
}