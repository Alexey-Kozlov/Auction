using Common.Contracts.Processing;

namespace Common.Contracts.Auction;

public interface IAuctionImageSplit
{
      bool UsingImage { get; set; }
      string Image { get; set; }
      bool IsImageSplitted { get; set; }
      Guid CorrelationId { get; set; }
}

public class AuctionItem
{
      public Guid? Id { get; set; }
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
      public Guid CorrelationId { get; set; }
      public bool Commited { get; set; }
}

public class AuctionImageDTO
{
      public string Image { get; set; }
      public bool UsingImage { get; set; }
      public bool IsImageSplitted { get; set; }
}

#region AuctionCreating

public class RequestAuctionCreate : IAuctionImageSplit
{
      public Guid AuctionId { get; set; }
      public int ReservePrice { get; set; }
      public DateTime AuctionEnd { get; set; }
      public string Properties { get; set; }
      public string Title { get; set; }
      public string Description { get; set; }
      public string Image { get; set; }
      public string UserLogin { get; set; }
      public Guid CorrelationId { get; set; }
      public bool UsingImage { get; set; }
      public bool IsImageSplitted { get; set; }
}


public class AuctionCreatedSearch : BaseServiceError, IFaultMessage { }

public class AuctionNotification
{
      public Guid AuctionId { get; set; }
      public string UserLogin { get; set; }
      public string Title { get; set; }
      public Guid CorrelationId { get; set; }
}

public class AuctionCreatedNotification : BaseServiceError, IFaultMessage { };


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
public class AuctionCreatedElk : BaseServiceError, IFaultMessage { };

public class AuctionCreateESCommit : BaseServiceError, IFaultMessage { };

public class AuctionCreateComplete : BaseServiceError, IFaultMessage { };

#endregion


#region AuctionDelete

public record RequestAuctionDelete(
      Guid CorrelationId,
      string UserLogin,
      Guid AuctionId
);


public class AuctionDeletedBid : BaseServiceError, IFaultMessage { };

public class AuctionDeletedGateway : BaseServiceError, IFaultMessage { };

public class AuctionDeletedImage : BaseServiceError, IFaultMessage { };

public class AuctionDeletedSearch : BaseServiceError, IFaultMessage { };

public class AuctionDeletedNotification : BaseServiceError, IFaultMessage { };

public class AuctionDeletedElk : BaseServiceError, IFaultMessage { };

public class AuctionDeleteESCommit : BaseServiceError, IFaultMessage { };

public class AuctionDeleteComplete : BaseServiceError, IFaultMessage { };

#endregion


#region AuctionUpdate
public class RequestAuctionUpdate : IAuctionImageSplit
{
      public Guid AuctionId { get; set; }
      public string Title { get; set; }
      public string Properties { get; set; }
      public string Image { get; set; }
      public string Description { get; set; }
      public string UserLogin { get; set; }
      public DateTime AuctionEnd { get; set; }
      public Guid CorrelationId { get; set; }
      public bool UsingImage { get; set; }
      public bool IsImageSplitted { get; set; } = false;
      public CRUD CRUD { get; set; } = CRUD.Update;
}


public class AuctionUpdatedGateWay : BaseServiceError, IFaultMessage { };

public class AuctionUpdatedSearch : BaseServiceError, IFaultMessage { };

public class AuctionUpdatedNotification : BaseServiceError, IFaultMessage { };

public class AuctionUpdatedElk : BaseServiceError, IFaultMessage { };

public class AuctionUpdateESCommit : BaseServiceError, IFaultMessage { };

public class AuctionUpdateComplete : BaseServiceError, IFaultMessage { };

public class AuctionUpdateFinalize : BaseServiceError, IFaultMessage { };

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
public class AuctionFinishedNotification : BaseServiceError, IFaultMessage { };

public record AuctionFinishingElk(
      Guid Id,
      bool ItemSold,
      string Winner,
      int Amount,
      Guid CorrelationId
);
public class AuctionFinishedElk : BaseServiceError, IFaultMessage { };

public class AuctionFinishedESCommit : BaseServiceError, IFaultMessage { };

public class AuctionFinishedComplete : BaseServiceError, IFaultMessage { };

#endregion

public class AuctionCommit
{
      public Guid CorrelationId { get; set; }
      public bool Commited { get; set; }
      public string CallBackType { get; set; }
}
