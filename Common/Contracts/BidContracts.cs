namespace Contracts;

public record RequestBidPlace(
     Guid AuctionId,
     string Bidder,
     int Amount,
     Guid CorrelationId
);
public record BidFinanceGranting(
     Guid AuctionId,
     string Bidder,
     int Amount,
     Guid CorrelationId
);
public record BidFinanceGranted(
     Guid CorrelationId
);
public record RollbackBidFinanceGranted(
    int Amount,
    Guid AuctionId,
    string Bidder,
    Guid CorrelationId
);
public record BidAuctionPlacing(
     Guid AuctionId,
     string Bidder,
     int Amount,
     Guid CorrelationId
);

public record BidAuctionPlaced(
     int OldHighBid,
     Guid CorrelationId
);
public record RollbackBidAuctionPlaced(
    int OldHighBid,
    Guid AuctionId,
    string Bidder,
    Guid CorrelationId
);

public record BidPlacing(
     Guid AuctionId,
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
     Guid AuctionId,
     int Amount,
     Guid CorrelationId
);
public record BidSearchPlaced(
     Guid CorrelationId
);
public record BidNotificationProcessing(
     Guid AuctionId,
     string Bidder,
     int Amount,
     Guid CorrelationId
);
public record BidNotificationProcessed(
     Guid CorrelationId
);


public record GetBidPlaceState(Guid CorrelationId);