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
      public Guid ItemId { get; set; }
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

#region AuctionCreating

public class RequestAuctionCreate : IAuctionImageSplit
{
      public Guid ItemId { get; set; }
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
      public string UserLogin { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
}

public class AuctionNotification
{
      public Guid ItemId { get; set; }
      public string UserLogin { get; set; }
      public string Title { get; set; }
      public Guid CorrelationId { get; set; }
      public bool Show { get; set; }
}

public class AuctionCreatedNotification : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};
public class AuctionCreatedNotificationEvent : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};


public class AuctionCreatingElk
{
      public Guid ItemId { get; set; }
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
      public int CurrentHighBid { get; set; }
};
public class AuctionCreatedElk : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionCreateESCommit : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionReset : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionCreateFinalize
{
      public Guid CorrelationId { get; set; }
};

#endregion


#region AuctionDelete

public record RequestAuctionDelete(
      string SessionId,
      string UserLogin,
      Guid ItemId,
      Guid CorrelationId
);


public class AuctionDeletedBid : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedGateway : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedImage : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedSearch : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedNotification : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedCommunication : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedNotificationEvent : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeletedElk : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionDeleteESCommit : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

#endregion


#region AuctionUpdate
public class RequestAuctionUpdate : IAuctionImageSplit
{
      public Guid ItemId { get; set; }
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
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionUpdatedSearch : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionUpdatedNotification : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionUpdatedNotificationEvent : IFaultMessage
{
      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionUpdatedElk : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionUpdateESCommit : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};


public class AuctionUpdateFinalize
{
      public Guid CorrelationId { get; set; }
};

#endregion


#region AuctionFinish


public record AuctionFinished(
    Guid CorrelationId
);


public class AuctionFinishedCommit : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionFinishedElk : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

public class AuctionFinishedNotification : IFaultMessage
{

      public Guid CorrelationId { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
      public string UserLogin { get; set; }
      public string CallBackType { get; set; }
      public Guid? ItemId { get; set; }
      public bool IsError { get; set; }
};

#endregion

public class AuctionCommit
{
      public Guid CorrelationId { get; set; }
      public bool Commited { get; set; }
      public string CallBackType { get; set; }
      public string UserLogin { get; set; }
      public string ErrorMessage { get; set; }
      public string ErrorExceptionMessage { get; set; }
      public string ErrorServiceName { get; set; }
}
