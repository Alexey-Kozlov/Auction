namespace ProcessingService.DTO;

public record CreateTagDTO(
    string Tag,
    Guid AuctionId
);