namespace ProcessingService.DTO;
public record PlaceBidDTO
(
     Guid Id,
     int Amount,
     Guid CorrelationId
);