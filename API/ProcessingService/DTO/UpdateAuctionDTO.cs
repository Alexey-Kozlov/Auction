namespace ProcessingService.DTO;
public record UpdateAuctionDTO
(
    Guid AuctionId,
    string Title,
    string Properties,
    string Description,
    string Image,
    int ReservePrice,
    DateTime AuctionEnd,
    Guid CorrelationId
);
