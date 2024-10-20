namespace Common.Contracts;

public record SendAllItems<T>(string UserLogin, string SessionId);

public class SendToSetSnapShot
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public string UserLogin { get; set; }
    public string SessionId { get; set; }
    public List<AuctionItem> AuctionItems { get; set; } = new();
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