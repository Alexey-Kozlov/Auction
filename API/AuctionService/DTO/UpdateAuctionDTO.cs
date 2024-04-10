namespace AuctionService.DTO;

public record UpdateAuctionDTO
(
    string Title,
    string Properties,
    string Description,
    string Image,
    int ReservePrice,
    DateTime AuctionEnd
);
