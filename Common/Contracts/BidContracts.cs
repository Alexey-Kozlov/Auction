namespace Contracts;

public record RequestProcessingBidStart(
     Guid AuctionId,
     string Bidder,
     int Amount,
     Guid CorrelationId
);

public record RequestBidPlace(
     Guid AuctionId,
     string Bidder,
     int Amount,
     Guid CorrelationId
);

public record BidPlaced(Guid CorrelationId, Guid AuctionId, int Amount);
public record GetProcessingBidState(Guid CorrelationId);