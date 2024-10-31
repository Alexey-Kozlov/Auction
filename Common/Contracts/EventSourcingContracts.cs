using System.Text.Json;

namespace Common.Contracts;

public class ESContract
{
    public string EventData { get; set; }
    public string EntityType { get; set; }
    public string ServiceName { get; set; }
    public Guid CorrelationId { get; set; }
    public string UserLogin { get; set; }
    public Guid? AuctionId { get; set; }
    public OperationType OperationType { get; set; }
}

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
    public string SessionId { get; set; }
    public List<string> SnapShotItems { get; set; } = new();
    public string ItemsType { get; set; }
    public string ServiceName { get; set; }
    public DateTime CreateAt { get; set; }
    public int RestoringOrder { get; set; }
}

public class SendToReindexingElk
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public string UserLogin { get; set; }
    public string SessionId { get; set; }
    public List<AuctionItem> AuctionItems { get; set; } = new();
}

public record EventSourcingInitialized(
    string Message,
    Guid CorrelationId,
    string UserLogin,
    string SessionId
);

public record RestoreSnapShotDb(
    string SessionId,
    string UserLogin,
    Guid SnapShotId
);

public class RestoreSnapShotItems<T>
{
    public string SessionId { get; set; }
    public string UserLogin { get; set; }
    public List<RestoreSnapShotItem> Items { get; set; }
}

public class RestoreSnapShotItem
{
    public List<JsonDocument> Items { get; set; }
    public string ItemsType { get; set; }
    public int? RestoringOrder { get; set; }
}

public record FinanceServiceType();
public record BiddingServiceType();
public record SearchServiceType();
public record NotificationServiceType();

public record RestoreSnapShotCompleted(
    string Message,
    Guid CorrelationId,
    string UserLogin,
    string SessionId
);
public record RequestCommitESOperation(Guid CorrelationId);
public record CommitESUpdateAuctionOperation(Guid CorrelationId);
public record CommitESCreateAuctionOperation(Guid CorrelationId);
public record CommitESDeleteAuctionOperation(Guid CorrelationId);

public record FinanceCreateMessage(
    FinanceItem FinanceItem,
    OperationType OperationType,
    Guid CorrelationId
);

public enum OperationType
{
    Delete, //0
    Update, //1
    Insert  //2
}