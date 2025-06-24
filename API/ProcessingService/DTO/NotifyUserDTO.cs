using System.ComponentModel.DataAnnotations;

namespace ProcessingService.DTO;

public record EditNotificationDTO
(
    [Required]
     Guid ItemId,
     bool Enable,
     string SessionId
);