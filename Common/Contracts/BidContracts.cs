namespace Contracts;

public record BidPlaced(
     Guid Id,
     Guid AuctionId,
     string Bidder,
    DateTime BidTime,
     int Amount,
     string BidStatus
);