namespace Common.Contracts;

public class AuctionItem
{
      public Guid AuctionId { get; set; }
      public int ReservePrice { get; set; }
      public string Seller { get; set; }
      public string Winner { get; set; }
      public int SoldAmount { get; set; }
      public int CurrentHighBid { get; set; }
      public DateTime CreateAt { get; set; }
      public DateTime UpdatedAt { get; set; }
      public DateTime AuctionEnd { get; set; }
      public string Title { get; set; }
      public string Properties { get; set; }
      public string Description { get; set; }
}

#region AuctionCreating

public record RequestAuctionCreate
(
       Guid AuctionId,
       int ReservePrice,
       DateTime AuctionEnd,
       string Properties,
       string Title,
       string Description,
       string Image,
       string UserLogin,
       Guid CorrelationId
);

public record AuctionCreatedBid(Guid CorrelationId);
public record AuctionCreatingBid(
      Guid AuctionId,
      DateTime AuctionEnd,
      string UserLogin,
      Guid CorrelationId,
      int ReservePrice
);

public record AuctionCreatingImage(
      Guid AuctionId,
      string Image,
      Guid CorrelationId
);
public record AuctionCreatedImage(Guid CorrelationId);

public record AuctionCreatingSearch(
      Guid AuctionId,
      string Title,
      string Properties,
      string Description,
      string UserLogin,
      DateTime AuctionEnd,
      Guid CorrelationId,
      int ReservePrice
);

public record AuctionCreatedSearch(Guid CorrelationId);

public record AuctionCreatingNotification(
      Guid AuctionId,
      string UserLogin,
      string Title,
      Guid CorrelationId
);

public record AuctionCreatedNotification(Guid CorrelationId);

public class AuctionCreatingElk
{
      public Guid AuctionId { get; set; }
      public string Title { get; set; }
      public string Properties { get; set; }
      public string Description { get; set; }
      public string UserLogin { get; set; }
      public DateTime AuctionEnd { get; set; }
      public DateTime AuctionCreated { get; set; }
      public Guid CorrelationId { get; set; }
      public int ReservePrice { get; set; }
      public bool ItemSold { get; set; }
      public string Winner { get; set; }
      public int Amount { get; set; }
};
public record AuctionCreatedElk(Guid CorrelationId);
public record AuctionCreateESCommit(Guid CorrelationId);

#endregion


#region AuctionDelete

public record RequestAuctionDelete(
      Guid CorrelationId,
      string UserLogin,
      Guid AuctionId
);
public record AuctionDeleteESFinance(Guid CorrelationId);
public record AuctionDeletingFinance(
    Guid AuctionId,
    string UserLogin,
    Guid CorrelationId
);
public record AuctionDeleteESBid(Guid CorrelationId);
public record AuctionDeletedFinance(Guid CorrelationId);

public record AuctionDeletingBid(
    Guid AuctionId,
    Guid CorrelationId
);
public record AuctionDeletedBid(Guid CorrelationId);
public record AuctionDeletingGateway(
    Guid AuctionId,
    Guid CorrelationId
);
public record AuctionDeletedGateway(Guid CorrelationId);

public record AuctionDeletingImage(
    Guid AuctionId,
    Guid CorrelationId
);
public record AuctionDeletedImage(Guid CorrelationId);
public record AuctionDeleteESSearch(Guid CorrelationId);
public record AuctionDeletingSearch(
    Guid AuctionId,
    Guid CorrelationId
);
public record AuctionDeletedSearch(Guid CorrelationId);
public record AuctionDeleteESNotification(Guid CorrelationId);
public record AuctionDeletingNotification(
    Guid AuctionId,
    string UserLogin,
    Guid CorrelationId
);
public record AuctionDeletedNotification(Guid CorrelationId);
public record AuctionDeletingElk(
    Guid AuctionId,
    string UserLogin,
    Guid CorrelationId
);
public record AuctionDeletedElk(Guid CorrelationId);
public record AuctionDeleteESCommit(Guid CorrelationId);

#endregion


#region AuctionUpdate
public record RequestAuctionUpdate(
      Guid AuctionId,
      string Title,
      string Properties,
      string Image,
      string Description,
      string UserLogin,
      DateTime AuctionEnd,
      Guid CorrelationId
);
public record AuctionUpdatingBid(
      Guid AuctionId,
      DateTime AuctionEnd,
      Guid CorrelationId
);
public record AuctionUpdatedBid(Guid CorrelationId);
public record AuctionUpdatingGateway(
      Guid AuctionId,
      Guid CorrelationId
);
public record AuctionUpdatedGateway(Guid CorrelationId);
public record AuctionUpdatingImage(
      Guid AuctionId,
      string Image,
      Guid CorrelationId
);
public record AuctionUpdatedImage(Guid CorrelationId);
public record AuctionUpdatingSearch(
      Guid AuctionId,
      string Title,
      string Properties,
      string Description,
      string UserLogin,
      DateTime AuctionEnd,
      Guid CorrelationId
);
public record AuctionUpdatedSearch(Guid CorrelationId);
public record AuctionUpdatingNotification(
      Guid AuctionId,
      string UserLogin,
      Guid CorrelationId
);
public record AuctionUpdatedNotification(Guid CorrelationId);
public record AuctionUpdatingElk(
      Guid AuctionId,
      string Title,
      string Properties,
      string Description,
      string UserLogin,
      DateTime AuctionEnd,
      Guid CorrelationId
);
public record AuctionUpdatedElk(Guid CorrelationId);

public record AuctionUpdateESCommit(Guid CorrelationId);

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

///////////////////////////////////////////////////
///
