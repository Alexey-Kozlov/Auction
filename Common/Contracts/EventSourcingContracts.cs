using System.Text.Json;

namespace Common.Contracts;

public class BaseStateContract
{
    public string EventData { get; set; }
    public string EntityType { get; set; }
    public string ServiceName { get; set; }
    public Guid CorrelationId { get; set; }
}


public class CommitAuctionUpdatingContract : BaseStateContract;

public record CommitAuctionUpdatedContract
(
    Guid CorrelationId
);

public class CommitAuctionCreatingContract : BaseStateContract { }
public record CommitAuctionCreatedContract
(
    Guid CorrelationId
);


public class CommitAuctionDeletingContract : BaseStateContract { }

public record CommitAuctionDeletedContract
(
    Guid CorrelationId
);

public record AfterBidPlacedContract
(
    Guid CorrelationId
);

public class CommitBidPlacingContract : BaseStateContract { }

public record CommitBidPlacedContract
(
    Guid CorrelationId
);
public record CommitBidPlacedErrorContract
(
    Guid CorrelationId,
    Exception ExceptionItem
);

public class CommitBidErrorContract : BaseStateContract
{

}

public record CommitErrorSavedContract
(
    Guid CorrelationId
);

public class CommitAuctionFinishingContract : BaseStateContract { }

public record CommitAuctionFinishedContract
(
    Guid CorrelationId
);


public record SendAllItems<T>(string UserLogin, string SessionId, Guid CorrelationId, DateTime CreateAt);

public class SendToSetSnapShot
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public string UserLogin { get; set; }
    public string SessionId { get; set; }
    public List<string> SnapShotItems { get; set; } = new();
    public string ItemsType { get; set; }
    public string ProjectName { get; set; }
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

public class AuctionItem
{
    public Guid Id { get; set; }
    public int ReservePrice { get; set; }
    public string Seller { get; set; }
    public string Winner { get; set; }
    public int SoldAmount { get; set; }
    public int CurrentHighBid { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime AuctionEnd { get; set; }
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
}

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

public record CommitESOperation(Guid CorrelationId);

