using System.ComponentModel.DataAnnotations;

namespace AuctionService.DTO;

public class CreateAuctionDTO
{
    [Required]
    public string Title { get; set; }
    [Required]
    public string Properties { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public int ReservePrice { get; set; }
    [Required]
    public DateTime AuctionEnd { get; set; }
}
