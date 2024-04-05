using System.ComponentModel.DataAnnotations;

namespace NotificationService.DTO;
public class NotifyUserDTO
{
    [Required]
    public Guid Id { get; set; }
    public bool Enable { get; set; }
}