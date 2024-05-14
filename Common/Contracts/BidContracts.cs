namespace Contracts;

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
public record RollbackBidFinanceGranted(
    Guid Id,
    string Bidder,
    int Amount,
    Guid CorrelationId
);
public record BidAuctionPlacing(
     Guid Id,
     string Bidder,
     int Amount,
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