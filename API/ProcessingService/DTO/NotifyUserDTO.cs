using System.ComponentModel.DataAnnotations;

namespace ProcessingService.DTO;
public record EditNotificationDTO
(
    [Required]
     Guid AuctionId,
     bool Enable,
     string SessionId
);