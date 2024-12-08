using System.ComponentModel.DataAnnotations;

namespace ProcessingService.DTO;

public class CreateAuctionDTO
{
    [Required]
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public int ReservePrice { get; set; }
    [Required]
    public DateTime AuctionEnd { get; set; }
    public Guid CorrelationId { get; set; }
    public bool UsingImage { get; set; }
}
