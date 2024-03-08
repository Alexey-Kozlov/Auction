using System.ComponentModel.DataAnnotations;

namespace AuctionService.DTO;

public class CreateAuctionDTO
{
    [Required]
    public string Make { get; set; }
    [Required]
    public string Model { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
    public int Mileage { get; set; }
    public string Description { get; set; }
    public string Image {get; set;}
    public int ReservePrice { get; set; }
    [Required]
    public DateTime AuctionEnd { get; set; }
}
