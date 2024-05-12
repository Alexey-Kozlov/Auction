namespace Contracts;


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

#endregion

public record AuctionFinishing(
      Guid Id,
      bool ItemSold,
      string Winner,
      string Seller,
      int Amount,
      Guid CorrelationId
);
public record AuctionFinished(
     Guid CorrelationId
);


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

#endregion

public record GetAuctionCreateState(Guid CorrelationId);
public record GetAuctionUpdateState(Guid CorrelationId);
public record GetAuctionDeleteState(Guid CorrelationId);
