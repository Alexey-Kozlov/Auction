using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionService.Entities;

public class Item
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }

    //nav properties
    public Auction Auction { get; set; }
    public Guid AuctionId { get; set; }
}
