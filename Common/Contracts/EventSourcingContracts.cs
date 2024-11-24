using System.Text.Json;
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
public record CommitESOperation(Guid CorrelationId);


public class ReturnResultSql
{
    public string eventdata { get; set; }
    public int crud { get; set; }
    public string entitytype { get; set; }
}
