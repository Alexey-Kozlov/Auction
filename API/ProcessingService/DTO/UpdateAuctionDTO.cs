namespace ProcessingService.DTO;
public record UpdateAuctionDTO
(
    string Id,
    string Title,
    string Properties,
    string Description,
    string Image,
    int ReservePrice,
    DateTime AuctionEnd,
    Guid CorrelationId
);
