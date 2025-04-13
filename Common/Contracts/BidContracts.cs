using Common.Contracts.Processing;

namespace Common.Contracts.Bid;

public record RequestBidPlace(
     Guid AuctionId,
     string Bidder,
     int Amount,
     Guid CorrelationId
);

public class BidFinanceGranted : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
};

public record RollbackBidFinanceGranted(
    Guid Id,
    string Bidder,
    int Amount,
    Guid CorrelationId
);

public record BidPlacing(
     Guid Id,
     string Bidder,
     int Amount,
     Guid CorrelationId
);
public class BidPlaced : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
};


public class BidSearchPlaced : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
};
public class BidNotification : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
};

public class BidNotificationEvent : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
};

public class BidCreateESCommit : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
};
public class BidComplete : IFaultMessage
{

     public Guid CorrelationId { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string UserLogin { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
};


public class BidItem
{
     public Guid? Id { get; set; }
     public Guid BidId { get; set; }
     public Guid AuctionId { get; set; }
     public string Bidder { get; set; }
     public DateTime BidTime { get; set; } = DateTime.UtcNow;
     public int Amount { get; set; }
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
}

public class BidCommit
{
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
     public string CallBackType { get; set; }
     public string UserLogin { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
}

public class BidReset : IFaultMessage
{
     public Guid CorrelationId { get; set; }
     public string UserLogin { get; set; }
     public string ErrorMessage { get; set; }
     public string ErrorExceptionMessage { get; set; }
     public string ErrorServiceName { get; set; }
     public string CallBackType { get; set; }
     public Guid? AuctionId { get; set; }
     public bool IsError { get; set; }
};
