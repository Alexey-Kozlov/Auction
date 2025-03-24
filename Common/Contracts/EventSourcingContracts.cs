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
    public Command Command { get; set; }
    public string Image { get; set; }
    public bool Commited { get; set; }
}

//указываем generic T для создания разных типов сообщений
public record SendAllItems<T>(
    string UserLogin,
    string SessionId,
    Guid CorrelationId,
    DateTime CreateAt
);

public class SendToSetSnapShot
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public string UserLogin { get; set; }
    public List<string> SnapShotItems { get; set; } = new();
    public string ItemsType { get; set; }
    public DateTime CreateAt { get; set; }
    public string SessionId { get; set; }
}


public record EventSourcingInitialized(
    string Message,
    Guid CorrelationId,
    string UserLogin,
    string SessionId
);

public class RequestRestoreItems
{
    public string UserLogin { get; set; }
    public DateTime RestoreDate { get; set; }
    public Guid CorrelationId { get; set; }
    public string SessionId { get; set; }
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

public class BidRestoreSnapShot
{
    public Guid CorrelationId { get; set; }
}
public class FinanceRestoreSnapShot
{
    public Guid CorrelationId { get; set; }
}
public class NotifyRestoreSnapShot
{
    public Guid CorrelationId { get; set; }
}
public class SearchRestoreSnapShot
{
    public Guid CorrelationId { get; set; }
}
public class ImageRestoreSnapShot
{
    public Guid CorrelationId { get; set; }
}
public class RestoreSnapShotESCommit
{
    public Guid CorrelationId { get; set; }
}
public class RestoreSnapShotComplete
{
    public Guid CorrelationId { get; set; }
}

public class RequestSetSnapShot
{
    public string UserLogin { get; set; }
    public Guid CorrelationId { get; set; }
    public string SessionId { get; set; }
}

public class BidSetSnapShot
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
}

public class FinanceSetSnapShot
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
}

public class NotifySetSnapShot
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
}

public class SearchSetSnapShot
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
}

public class ImageSetSnapShot
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
}
public class SetSnapShotESCommit
{
    public Guid CorrelationId { get; set; }
}
public class SetSnapShotComplete
{
    public Guid CorrelationId { get; set; }
    public DataForProcessingServicesList DataItems { get; set; }
    public int AllItemsCount { get; set; }
}

public class AuctionFinishedData
{
    public Guid AuctionId { get; set; }
    public DateTime AuctionEnd { get; set; }
}