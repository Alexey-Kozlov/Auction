namespace ProcessingService.DTO;

public record DeleteTagDTO(
    string Tag,
    Guid AuctionId
);