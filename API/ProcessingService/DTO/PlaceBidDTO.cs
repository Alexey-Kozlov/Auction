namespace ProcessingService.DTO;
public record PlaceBidDTO
(
     Guid auctionId,
     int amount,
     Guid correlationId
);