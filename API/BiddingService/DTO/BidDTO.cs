namespace BiddingService.DTO;

public class BidDTO
{
     public Guid AuctionId { get; set; }
     public Guid BidId { get; set; }
     public string Bidder { get; set; }
     public DateTime BidTime { get; set; }
     public int Amount { get; set; }
}
