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


public class AuctionCreatedSearch : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
}

public class AuctionNotification
{
      public Guid AuctionId { get; set; }
      public string UserLogin { get; set; }
      public string Title { get; set; }
      public Guid CorrelationId { get; set; }
      public bool Show { get; set; }
}

public class AuctionCreatedNotification : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};
public class AuctionCreatedNotificationEvent : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};


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
public class AuctionCreatedElk : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionCreateESCommit : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

#endregion


#region AuctionDelete

public record RequestAuctionDelete(
      Guid CorrelationId,
      string UserLogin,
      Guid AuctionId
);


public class AuctionDeletedBid : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedGateway : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedImage : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedSearch : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedNotification : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedNotificationEvent : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedElk : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeleteESCommit : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeleteComplete : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

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


public class AuctionUpdatedGateWay : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionUpdatedSearch : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionUpdatedNotification : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionUpdatedNotificationEvent : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionUpdatedElk : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionUpdateESCommit : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionUpdateComplete : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionUpdateFinalize : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

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
public class AuctionFinishedNotification : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public record AuctionFinishingElk(
      Guid Id,
      bool ItemSold,
      string Winner,
      int Amount,
      Guid CorrelationId
);
public class AuctionFinishedElk : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionFinishedESCommit : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionFinishedComplete : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};

#endregion

public class AuctionCommit
{
      public Guid CorrelationId { get; set; }
      public bool Commited { get; set; }
      public string CallBackType { get; set; }
}

public class AuctionError : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string Message { get; set; }
      public string ExceptionMessage { get; set; }
      public string UserLogin { get; set; }
      public string ServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? AuctionId { get; set; }
      public bool IsError { get; set; }
};