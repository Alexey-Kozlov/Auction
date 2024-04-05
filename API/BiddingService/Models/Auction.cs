namespace BiddingService.Models;

public class Auction
{
    public Guid Id { get; set; }
    public DateTime AuctionEnd { get; set; }
    public string Seller { get; set; }
    public int ReservePrice { get; set; }
    public bool Finished { get; set; }
    public ICollection<Bid> Bids { get; set; }
}
