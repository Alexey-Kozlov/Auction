namespace Contracts;

public class AuctionCreated
{
      public Guid Id { get; set; }
      public int ReservePrice { get; set; }
      public string Seller { get; set; }
      public string Winner { get; set; }
      public int SoldAmount { get; set; }
      public int CurrentHighBid { get; set; }
      public DateTime CreateAt { get; set; }
      public DateTime UpdatedAt { get; set; }
      public DateTime AuctionEnd { get; set; }
      public string Status { get; set; }
      public string Properties { get; set; }
      public string Title { get; set; }
      public string Description { get; set; }
      public string Image { get; set; }
      public string AuctionAuthor { get; set; }
      public Guid CorrelationId { get; set; }
};

#region AuctionDelete
public record RequestAuctionDelete(
      Guid CorrelationId,
      string AuctionAuthor,
      Guid Id
);
public record AuctionDeleting(
    Guid Id,
    string AuctionAuthor,
    Guid CorrelationId
);
public record AuctionDeleted(
    Guid Id,
    Guid CorrelationId
);

public record AuctionDeletingBid(
    Guid Id,
    Guid CorrelationId
);
public record AuctionDeletedBid(
    Guid Id,
    Guid CorrelationId
);
public record AuctionDeletingGateway(
    Guid Id,
    Guid CorrelationId
);
public record AuctionDeletedGateway(
    Guid Id,
    Guid CorrelationId
);

public record AuctionDeletingImage(
    Guid Id,
    Guid CorrelationId
);
public record AuctionDeletedImage(
    Guid Id,
    Guid CorrelationId
);
public record AuctionDeletingSearch(
    Guid Id,
    Guid CorrelationId
);
public record AuctionDeletedSearch(
    Guid Id,
    Guid CorrelationId
);
public record AuctionDeletingNotification(
    Guid Id,
    Guid CorrelationId
);
public record AuctionDeletedNotification(
    Guid Id,
    Guid CorrelationId
);

#endregion

public record AuctionFinishing(
     bool ItemSold,
     Guid Id,
     string Winner,
     string Seller,
     int Amount,
     Guid CorrelationId
);
public record AuctionFinished(
     Guid Id,
     Guid CorrelationId
);


#region AuctionUpdating

public record RequestAuctionUpdate(
      string Id,
      string Title,
      string Properties,
      string Image,
      string Description,
      string AuctionAuthor,
      DateTime AuctionEnd,
      Guid CorrelationId
);

public record AuctionUpdating(
      string Id,
      string Title,
      string Properties,
      string Image,
      string Description,
      string AuctionAuthor,
      DateTime AuctionEnd,
      Guid CorrelationId
);

public record AuctionUpdated(
      string Id,
      string Title,
      string Properties,
      string Image,
      string Description,
      string AuctionAuthor,
      DateTime AuctionEnd,
      Guid CorrelationId
);

public record AuctionUpdatingBid(
      string Id,
      DateTime AuctionEnd,
      Guid CorrelationId
);

public record AuctionUpdatedBid(
      string Id,
      Guid CorrelationId
);
public record AuctionUpdatingGateway(
      string Id,
      Guid CorrelationId
);
public record AuctionUpdatedGateway(
      string Id,
      Guid CorrelationId
);
public record AuctionUpdatingImage(
      string Id,
      string Image,
      Guid CorrelationId
);
public record AuctionUpdatedImage(
      string Id,
      Guid CorrelationId
);

public record AuctionUpdatingSearch(
      string Id,
      string Title,
      string Properties,
      string Description,
      string AuctionAuthor,
      DateTime AuctionEnd,
      Guid CorrelationId
);

public record AuctionUpdatedSearch(
      string Id,
      Guid CorrelationId
);

public record AuctionUpdatingNotification(
      string Id,
      string AuctionAuthor,
      Guid CorrelationId
);

public record AuctionUpdatedNotification(
      string Id,
      Guid CorrelationId
);

#endregion
