namespace AuctionService.DTO;
public record UpdateAuctionDTO
(
    Guid Id,
    string Title,
    string Properties,
    string Description,
    string Image,
    int ReservePrice,
    DateTime AuctionEnd,
    Guid CorrelationId
);
