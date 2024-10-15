using System.ComponentModel.DataAnnotations;

namespace ProcessingService.DTO;

public record CreateAuctionDTO
(
    [Required]
     string Title,
     string Properties,
     string Description,
     string Image,
     int ReservePrice,
    [Required]
     DateTime AuctionEnd,
     Guid CorrelationId
);
