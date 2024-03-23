using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionService.Entities;

public class Item
{
    public Guid Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; } = 0;
    public string Color { get; set; }
    public int Mileage { get; set; } = 0;
    public string Description { get; set; }

    //nav properties
    public Auction Auction { get; set; }
    public Guid AuctionId { get; set; }
}
