namespace ProcessingService.DTO;

public record BidDTO(

     string Id,
     string AuctionId,
     string Bidder,
     DateTime BidTime,
     int Amount,
     string BidStatus
);
