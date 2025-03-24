using Common.Contracts.Processing;

namespace Common.Contracts.Bid;

public record RequestBidPlace(
     Guid AuctionId,
     string Bidder,
     int Amount,
     Guid CorrelationId
);

public class BidFinanceGranted : BaseServiceError, IFaultMessage { };

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
public class BidPlaced : BaseServiceError, IFaultMessage { };


public class BidSearchPlaced : BaseServiceError, IFaultMessage { };

public class BidNotificationProcessed : BaseServiceError, IFaultMessage { };
public class BidCreateESCommit : BaseServiceError, IFaultMessage { };
public class BidComplete : BaseServiceError, IFaultMessage { };


public class BidItem
{
     public Guid? Id { get; set; }
     public Guid BidId { get; set; }
     public Guid AuctionId { get; set; }
     public string Bidder { get; set; }
     public DateTime BidTime { get; set; } = DateTime.UtcNow;
     public int Amount { get; set; }
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
}

public class BidCommit
{
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
     public string CallBackType { get; set; }
}