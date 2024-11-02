using System.Text.Json.Serialization;

namespace Common.Contracts;

public record RequestBidPlace(
     Guid Id,
     string Bidder,
     int Amount,
     Guid CorrelationId
);
public record BidFinanceGranting(
     Guid Id,
     string Bidder,
     int Amount,
     Guid CorrelationId
);
public record BidFinanceGranted(
     Guid CorrelationId
);
public record GetCurrentBid(
     Guid CorrelationId,
     int CurrentHighBid
);
public record RollbackBidFinanceGranted(
    Guid Id,
    string Bidder,
    int Amount,
    Guid CorrelationId
);
public record GetLastBidPlaced(
     Guid Id,
     Guid CorrelationId
);

public record BidAuctionPlaced(
     int OldHighBid,
     Guid CorrelationId
);
public record RollbackBidAuctionPlaced(
    Guid Id,
    int OldHighBid,
    string Bidder,
    Guid CorrelationId
);

public record BidPlacing(
     Guid Id,
     string Bidder,
     int Amount,
     Guid CorrelationId
);
public record BidPlaced(
     Guid BidId,
     Guid CorrelationId
);
public record RollbackBidPlaced(
    Guid BidId,
    Guid CorrelationId
);
public record BidSearchPlacing(
     Guid Id,
     string Bidder,
     int Amount,
     Guid CorrelationId
);
public record BidSearchPlaced(
     Guid CorrelationId
);
public record BidNotificationProcessing(
     Guid Id,
     string Bidder,
     int Amount,
     Guid CorrelationId
);
public record BidNotificationProcessed(
     Guid CorrelationId
);


public record GetBidPlaceState(Guid CorrelationId);

public class AuctionBidItem
{
     public Guid AuctionId { get; set; }
     public DateTime AuctionEnd { get; set; }
     public string Seller { get; set; }
     public int ReservePrice { get; set; }
     public bool Finished { get; set; }
     public ICollection<BidItem> Bids { get; set; }
}

public class BidItem
{
     public Guid BidId { get; set; }
     public Guid AuctionId { get; set; }
     public string Bidder { get; set; }
     public DateTime BidTime { get; set; } = DateTime.UtcNow;
     public int Amount { get; set; }
     [JsonIgnore]
     public AuctionBidItem Auction { get; set; }
}


