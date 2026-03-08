namespace ProcessingService.DTO;

public record CreateTagDTO(
    string Name,
    Guid AuctionId
);