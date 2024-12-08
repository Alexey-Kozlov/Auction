namespace Common.Contracts.Auction;

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
      public bool Finished { get; set; } = false;
}

public class AuctionImageDTO
{
      public string Image { get; set; }
      public bool UsingImage { get; set; }
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
       Guid CorrelationId,
       bool UsingImage
);


public class AuctionCreatedSearch
{
      public Guid CorrelationId { get; set; }
}

public class AuctionNotification
{
      public Guid AuctionId { get; set; }
      public string UserLogin { get; set; }
      public string Title { get; set; }
      public Guid CorrelationId { get; set; }
}

public class AuctionCreatedNotification
{
      public Guid CorrelationId { get; set; }
}

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
public class AuctionCreatedElk
{
      public Guid CorrelationId { get; set; }
}
public class AuctionCreateESCommit
{
      public Guid CorrelationId { get; set; }
}
public class AuctionCreateComplete
{
      public Guid CorrelationId { get; set; }
}
#endregion


#region AuctionDelete

public record RequestAuctionDelete(
      Guid CorrelationId,
      string UserLogin,
      Guid AuctionId
);


public class AuctionDeletedBid
{
      public Guid CorrelationId { get; set; }
}

public class AuctionDeletedGateway
{
      public Guid CorrelationId { get; set; }
}


public class AuctionDeletedImage
{
      public Guid CorrelationId { get; set; }
}


public class AuctionDeletedSearch
{
      public Guid CorrelationId { get; set; }
}


public class AuctionDeletedNotification
{
      public Guid CorrelationId { get; set; }
}

public class AuctionDeletedElk
{
      public Guid CorrelationId { get; set; }
}
public class AuctionDeleteESCommit
{
      public Guid CorrelationId { get; set; }
}

public class AuctionDeleteComplete
{
      public Guid CorrelationId { get; set; }
}

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
      Guid CorrelationId,
      bool UsingImage
);


public record AuctionUpdatedImage
{
      public Guid CorrelationId { get; set; }
}

public class AuctionUpdatedSearch
{
      public Guid CorrelationId { get; set; }
}

public record AuctionUpdatedNotification
{
      public Guid CorrelationId { get; set; }
}

public record AuctionUpdatedElk
{
      public Guid CorrelationId { get; set; }
}

public class AuctionUpdateESCommit
{
      public Guid CorrelationId { get; set; }
}

public class AuctionUpdateComplete
{
      public Guid CorrelationId { get; set; }
}

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
public class AuctionFinishedNotification
{
      public Guid CorrelationId { get; set; }
}
public record AuctionFinishingElk(
      Guid Id,
      bool ItemSold,
      string Winner,
      int Amount,
      Guid CorrelationId
);
public class AuctionFinishedElk
{
      public Guid CorrelationId { get; set; }
}
public class AuctionFinishedESCommit
{
      public Guid CorrelationId { get; set; }
}


public class AuctionFinishedComplete
{
      public Guid CorrelationId { get; set; }
}

#endregion

