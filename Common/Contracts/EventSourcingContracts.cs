using Common.Contracts.Processing;

namespace Common.Contracts.EventSourcing;

public class ESContract
{
    public string EventData { get; set; }
    public string EntityType { get; set; }
    public string CallBackType { get; set; }
    public Guid CorrelationId { get; set; }
    public string UserLogin { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? ItemId { get; set; }
    public Command Command { get; set; }
    public string Image { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public bool IsError { get; set; }
}


public class RequestRestoreItems
{
    public string UserLogin { get; set; }
    public DateTime RestoreDate { get; set; }
    public Guid CorrelationId { get; set; }
    public bool ResetLog { get; set; }
}

public class RequestRestoreImages
{
    public string UserLogin { get; set; }
    public DateTime RestoreDate { get; set; }
    public Guid CorrelationId { get; set; }
    public int StartNumber { get; set; }
    public int MaxMessageSizeMb { get; set; }
}


public record RequestCommitESOperation(Guid CorrelationId);
public record CommitESOperation(Guid CorrelationId);


public class ReturnResultSql
{
    public string eventdata { get; set; }
    public int crud { get; set; }
    public string entitytype { get; set; }
}

public class ReturnRestoreResultSql
{
    public bool res { get; set; }
}

public class BidRestoreSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? ItemId { get; set; }

}
public class FinanceRestoreSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? ItemId { get; set; }

}
public class NotifyRestoreSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? ItemId { get; set; }

}

public class CommunicationRestoreSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? ItemId { get; set; }

}

public class NotifyUIRestoreSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? ItemId { get; set; }

}

public class SearchRestoreSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? ItemId { get; set; }

}

public class TagRestoreSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? ItemId { get; set; }

}


public class RestoreSnapShotESCommit : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? ItemId { get; set; }

}

public class RequestSetSnapShot
{
    public string UserLogin { get; set; }
    public Guid CorrelationId { get; set; }
}

public class BidSetSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? ItemId { get; set; }

}

public class FinanceSetSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? ItemId { get; set; }

}

public class NotifySetSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? ItemId { get; set; }

}

public class NotifyUISetSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? ItemId { get; set; }

}

public class SearchSetSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? ItemId { get; set; }

}

public class ImageSetSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? ItemId { get; set; }

}

public class CommunicationSetSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? ItemId { get; set; }

}

public class TagSetSnapShot : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? ItemId { get; set; }

}

public class SetSnapShotESCommit : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public string CallBackType { get; set; }
    public Guid? ItemId { get; set; }

}

public class AuctionFinishedData
{
    public Guid? ItemId { get; set; }
    public DateTime AuctionEnd { get; set; }
}

public class SendStartFinishService : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string CallBackType { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public Guid? ItemId { get; set; }
}

public class SendStopFinishService : IFaultMessage
{
    public Guid CorrelationId { get; set; }
    public string CallBackType { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorExceptionStack { get; set; }
    public string ErrorExceptionInputData { get; set; }
    public string ErrorServiceName { get; set; }
    public string UserLogin { get; set; }
    public Guid? ItemId { get; set; }
}

public class StartFinishService
{
    public Guid CorrelationId { get; set; }
    public string CallBackType { get; set; }
}

public class StopFinishService
{
    public Guid CorrelationId { get; set; }
    public string CallBackType { get; set; }
}