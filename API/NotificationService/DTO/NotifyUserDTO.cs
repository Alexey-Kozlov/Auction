using System.ComponentModel.DataAnnotations;

namespace NotificationService.DTO;
public record NotifyUserDTO
(
    [Required]
     Guid Id,
     bool Enable
);