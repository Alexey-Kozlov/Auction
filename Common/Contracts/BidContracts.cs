using Common.Contracts.Processing;

namespace Common.Contracts.Bid;

public record RequestBidPlace(
     Guid AuctionId,
     string Bidder,
     int Amount,
     Guid CorrelationId
);

public class BidFinanceGranted : IFaultMessage
{
     public Guid CorrelationId { get; set; }
};

public record RollbackBidFinanceGranted(
    Guid Id,
    string Bidder,
    int Amount,
    Guid CorrelationId
);

public record BidPlacing(
     Guid Id,
     string Bidder,
     int Amount,
     Guid CorrelationId
);
public class BidPlaced
{
     public Guid CorrelationId { get; set; }
};


public class BidSearchPlaced
{
     public Guid CorrelationId { get; set; }
};

public class BidNotificationProcessed
{
     public Guid CorrelationId { get; set; }
};
public class BidCreateESCommit
{
     public Guid CorrelationId { get; set; }
};
public class BidComplete
{
     public Guid CorrelationId { get; set; }
};


public class BidItem
{
     public Guid BidId { get; set; }
     public Guid AuctionId { get; set; }
     public string Bidder { get; set; }
     public DateTime BidTime { get; set; } = DateTime.UtcNow;
     public int Amount { get; set; }
}

