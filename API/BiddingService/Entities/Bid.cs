using System.Text.Json.Serialization;

namespace BiddingService.Entities;

public class Bid
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public string Bidder { get; set; }
    public DateTime BidTime { get; set; } = DateTime.UtcNow;
    public int Amount { get; set; }
    public BidStatus BidStatus { get; set; }
    [JsonIgnore]
    public Auction Auction { get; set; }
}
