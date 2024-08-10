namespace Common.Contracts;


#region AuctionCreating

public record RequestAuctionCreate
(
       Guid Id,
       int ReservePrice,
       DateTime AuctionEnd,
       string Properties,
       string Title,
       string Description,
       string Image,
       string AuctionAuthor,
       Guid CorrelationId
);

public record AuctionCreating(
      Guid Id,
      string Title,
      string Properties,
      string Image,
      string Description,
      string AuctionAuthor,
      DateTime AuctionEnd,
      Guid CorrelationId,
      int ReservePrice
);

public record AuctionCreated(
      Guid CorrelationId
);

public record AuctionCreatingBid(
      Guid Id,
      DateTime AuctionEnd,
      string AuctionAuthor,
      Guid CorrelationId,
      int ReservePrice
);

public record AuctionCreatedBid(
      Guid CorrelationId
);
public record AuctionCreatingImage(
      Guid Id,
      string Image,
      Guid CorrelationId
);
public record AuctionCreatedImage(
      Guid CorrelationId
);

public record AuctionCreatingSearch(
      Guid Id,
      string Title,
      string Properties,
      string Description,
      string AuctionAuthor,
      DateTime AuctionEnd,
      Guid CorrelationId,
      int ReservePrice
);

public record AuctionCreatedSearch(
      Guid CorrelationId
);

public record AuctionCreatingNotification(
      Guid Id,
      string AuctionAuthor,
      string Title,
      Guid CorrelationId
);

public record AuctionCreatedNotification(
      Guid CorrelationId
);

public class AuctionCreatingElk
{
      public Guid Id { get; set; }
      public string Title { get; set; }
      public string Properties { get; set; }
      public string Description { get; set; }
      public string AuctionAuthor { get; set; }
      public DateTime AuctionEnd { get; set; }
      public DateTime AuctionCreated { get; set; }
      public Guid CorrelationId { get; set; }
      public int ReservePrice { get; set; }
      public bool ItemSold { get; set; }
      public string Winner { get; set; }
      public int Amount { get; set; }
};
public record AuctionCreatedElk(
      Guid CorrelationId
);

#endregion


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
    Guid CorrelationId
);
public record AuctionDeletingFinance(
    Guid Id,
    string AuctionAuthor,
    Guid CorrelationId
);
public record AuctionDeletedFinance(
    Guid CorrelationId
);

public record AuctionDeletingBid(
    Guid Id,
    Guid CorrelationId
);
public record AuctionDeletedBid(
    Guid CorrelationId
);
public record AuctionDeletingGateway(
    Guid Id,
    Guid CorrelationId
);
public record AuctionDeletedGateway(
    Guid CorrelationId
);

public record AuctionDeletingImage(
    Guid Id,
    Guid CorrelationId
);
public record AuctionDeletedImage(
    Guid CorrelationId
);
public record AuctionDeletingSearch(
    Guid Id,
    Guid CorrelationId
);
public record AuctionDeletedSearch(
    Guid CorrelationId
);
public record AuctionDeletingNotification(
    Guid Id,
    string AuctionAuthor,
    Guid CorrelationId
);
public record AuctionDeletedNotification(
    Guid CorrelationId
);
public record AuctionDeletingElk(
    Guid Id,
    string AuctionAuthor,
    Guid CorrelationId
);
public record AuctionDeletedElk(
    Guid CorrelationId
);

#endregion


#region AuctionUpdate

public record RequestAuctionUpdate(
      Guid Id,
      string Title,
      string Properties,
      string Image,
      string Description,
      string AuctionAuthor,
      DateTime AuctionEnd,
      Guid CorrelationId
);

public record AuctionUpdating(
      Guid Id,
      string Title,
      string Properties,
      string Image,
      string Description,
      string AuctionAuthor,
      DateTime AuctionEnd,
      Guid CorrelationId
);

public record AuctionUpdated(
      Guid CorrelationId
);

public record AuctionUpdatingBid(
      Guid Id,
      DateTime AuctionEnd,
      Guid CorrelationId
);

public record AuctionUpdatedBid(
      Guid CorrelationId
);
public record AuctionUpdatingGateway(
      Guid Id,
      Guid CorrelationId
);
public record AuctionUpdatedGateway(
      Guid CorrelationId
);
public record AuctionUpdatingImage(
      Guid Id,
      string Image,
      Guid CorrelationId
);
public record AuctionUpdatedImage(
      Guid CorrelationId
);
public record AuctionUpdatingSearch(
      Guid Id,
      string Title,
      string Properties,
      string Description,
      string AuctionAuthor,
      DateTime AuctionEnd,
      Guid CorrelationId
);

public record AuctionUpdatedSearch(
      Guid CorrelationId
);

public record AuctionUpdatingNotification(
      Guid Id,
      string AuctionAuthor,
      Guid CorrelationId
);

public record AuctionUpdatedNotification(
      Guid CorrelationId
);
public record AuctionUpdatingElk(
      Guid Id,
      string Title,
      string Properties,
      string Description,
      string AuctionAuthor,
      DateTime AuctionEnd,
      Guid CorrelationId
);

public record AuctionUpdatedElk(
      Guid CorrelationId
);
#endregion


#region AuctionFinish
public record RequestAuctionFinish(
      Guid Id,
      bool ItemSold,
      string Winner,
      int Amount,
      Guid CorrelationId
);
public record AuctionFinishing(
      Guid Id,
      bool ItemSold,
      string Winner,
      int Amount,
      Guid CorrelationId
);
public record AuctionFinished(
    Guid CorrelationId
);
public record AuctionFinishingFinance(
    Guid Id,
    bool ItemSold,
    string Winner,
    Guid CorrelationId
);
public record AuctionFinishedFinance(
    Guid CorrelationId
);

public record AuctionFinishingSearch(
      Guid Id,
      bool ItemSold,
      string Winner,
      int Amount,
      Guid CorrelationId
);
public record AuctionFinishedSearch(
    Guid CorrelationId
);
public record AuctionFinishingNotification(
      Guid Id,
      bool ItemSold,
      string Winner,
      int Amount,
      Guid CorrelationId
);
public record AuctionFinishedNotification(
    Guid CorrelationId
);
public record AuctionFinishingElk(
      Guid Id,
      bool ItemSold,
      string Winner,
      int Amount,
      Guid CorrelationId
);
public record AuctionFinishedElk(
    Guid CorrelationId
);

#endregion



public record GetAuctionCreateState(Guid CorrelationId);
public record GetAuctionUpdateState(Guid CorrelationId);
public record GetAuctionDeleteState(Guid CorrelationId);
public record GetAuctionFinishState(Guid CorrelationId);
