namespace AuctionService.Entities;

public class Auction
{
    public Guid Id { get; set; }
    public int ReservePrice { get; set; } = 0;
    public string Seller { get; set; }
    public string Winner { get; set; }
    public int SoldAmount { get; set; } = 0;
    public int CurrentHighBid { get; set; } = 0;
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AuctionEnd { get; set; }
    public Status Status { get; set; } = Status.Начался;
    public ICollection<Item> Item { get; set; }

}
