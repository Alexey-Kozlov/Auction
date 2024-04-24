namespace Contracts;

public record AuctionCreated(
   Guid Id,
   int ReservePrice,
   string Seller,
   string Winner,
    int SoldAmount,
    int CurrentHighBid,
    DateTime CreateAt,
    DateTime UpdatedAt,
    DateTime AuctionEnd,
    string Status,
    string Properties,
    string Title,
    string Description,
    string Image
);

public record AuctionDeleted(
    Guid Id
);

public record AuctionFinished(
     bool ItemSold,
     Guid AuctionId,
     string Winner,
     string Seller,
     int Amount
);

public class AuctionUpdated
{
     public string Id { get; set; }
     public string Title { get; set; }
     public string Properties { get; set; }
     public string Image { get; set; }
     public string Description { get; set; }
     public DateTime AuctionEnd { get; set; }
};

