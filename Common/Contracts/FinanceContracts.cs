using Common.Contracts.Processing;

namespace Common.Contracts.Finance;

public record FinanceCreate(
     int Amount,
     string UserLogin
);
public record RequestCreateFinance(
     int Amount,
     string UserLogin,
     Guid CorrelationId,
     string SessionId
);

public class FinanceCreated : IFaultMessage
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

public class FinanceCreateComplete : IFaultMessage
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

public class FinanceNotificationCreated : IFaultMessage
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

public class FinanceCreateESCommit : IFaultMessage
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

public class FinanceCommit
{
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
     public string CallBackType { get; set; }
}

public class FinanceItem
{
     public Guid? Id { get; set; }
     public Guid FinanceId { get; set; }
     public Guid? AuctionId { get; set; }
     public string UserLogin { get; set; }
     public int Value { get; set; }
     public DateTime ActionDate { get; set; }
     public FinanceRecordStatus Status { get; set; }
     public Guid CorrelationId { get; set; }
     public bool Commited { get; set; }
}


public enum FinanceRecordStatus
{
     Приход,
     Расход,
     Баланс
}
