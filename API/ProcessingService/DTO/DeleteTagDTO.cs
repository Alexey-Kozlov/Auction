namespace ProcessingService.DTO;

public record DeleteTagDTO(
    string Name,
    Guid AuctionId
);