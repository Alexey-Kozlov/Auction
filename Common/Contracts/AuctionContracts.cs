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

public record AuctionUpdated(
     string Id,
     string Title,
     string Properties,
     string Image,
     string Description,
     DateTime AuctionEnd
);

